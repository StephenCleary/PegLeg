using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PegLeg
{
    public sealed class PegParser
    {
        public ParseResult? TryParse(ReadOnlyMemory<char> input)
        {
            return Identifier(input);

            ParseResult? Identifier(ReadOnlyMemory<char> input)
            {
                // (LowercaseLetter / UppercaseLetter / Underscore) (LowercaseLetter / UppercaseLetter / Underscore / Digit)*
                var firstLetter = Choice(LowercaseLetter, UppercaseLetter, Underscore);
                var otherLetters = Star(Choice(LowercaseLetter, UppercaseLetter, Underscore, Digit));
                return Sequence(firstLetter, otherLetters)(input);
            }

            ParseResult? LowercaseLetter(ReadOnlyMemory<char> input) => CharacterRange('a', 'z')(input);
            ParseResult? UppercaseLetter(ReadOnlyMemory<char> input) => CharacterRange('A', 'Z')(input);
            ParseResult? Digit(ReadOnlyMemory<char> input) => CharacterRange('0', '9')(input);
            ParseResult? Underscore(ReadOnlyMemory<char> input) => ExactString("_", caseInsensitive: false)(input);
        }

        private Func<ReadOnlyMemory<char>, ParseResult?> PositiveLookahead(Func<ReadOnlyMemory<char>, ParseResult?> child)
        {
            return input =>
            {
                var result = child(input);
                if (result == null)
                    return ParseResult.Fail();
                return ParseResult.Empty(input);
            };
        }

        private Func<ReadOnlyMemory<char>, ParseResult?> NegativeLookahead(Func<ReadOnlyMemory<char>, ParseResult?> child)
        {
            return input =>
            {
                var result = child(input);
                if (result == null)
                    return ParseResult.Empty(input);
                return ParseResult.Fail();
            };
        }

        private Func<ReadOnlyMemory<char>, ParseResult?> Sequence(params Func<ReadOnlyMemory<char>, ParseResult?>[] children)
        {
            return input =>
            {
                int offset = 0;
                foreach (var child in children)
                {
                    var childInput = input.Slice(offset);
                    var childResult = child(childInput);
                    if (childResult == null)
                        return ParseResult.Fail();
                    offset += childResult.Value.Memory.Length;
                }

                return ParseResult.Create(input, offset);
            };
        }

        private Func<ReadOnlyMemory<char>, ParseResult?> Optional(Func<ReadOnlyMemory<char>, ParseResult?> child)
        {
            return input =>
            {
                var result = child(input);
                if (result != null)
                    return result;

                return ParseResult.Empty(input);
            };
        }

        private Func<ReadOnlyMemory<char>, ParseResult?> Plus(Func<ReadOnlyMemory<char>, ParseResult?> child) => Sequence(child, Star(child));

        private Func<ReadOnlyMemory<char>, ParseResult?> Star(Func<ReadOnlyMemory<char>, ParseResult?> child)
        {
            return input =>
            {
                var offset = 0;
                while (true)
                {
                    var childInput = input.Slice(offset);
                    var childResult = child(childInput);
                    if (childResult == null)
                        break;
                    offset += childResult.Value.Memory.Length;
                }

                return ParseResult.Create(input, offset);
            };
        }

        private Func<ReadOnlyMemory<char>, ParseResult?> Quantify(Func<ReadOnlyMemory<char>, ParseResult?> child, int min, int? inclusiveMax, Func<ReadOnlyMemory<char>, ParseResult?>? delimiter)
        {
            if (min == 0 && inclusiveMax == 1)
                return Optional(child);
            if (delimiter == null && min == 0 && inclusiveMax == null)
                return Star(child);
            if (delimiter == null && min == 1 && inclusiveMax == null)
                return Plus(child);
            if (delimiter == null && inclusiveMax != null && min == inclusiveMax)
                return Sequence(Enumerable.Repeat(child, min).ToArray());

            return input =>
            {
                var offset = 0;
                var count = 0;
                while (inclusiveMax == null || count < inclusiveMax.Value)
                {
                    var offsetBeforeDelimiter = offset;
                    if (delimiter != null && count != 0)
                    {
                        var delimiterResult = delimiter(input.Slice(offset));
                        if (delimiterResult == null)
                            break;
                        offset += delimiterResult.Value.Memory.Length;
                    }

                    var childInput = input.Slice(offset);
                    var childResult = child(childInput);
                    if (childResult == null)
                    {
                        offset = offsetBeforeDelimiter;
                        break;
                    }

                    offset += childResult.Value.Memory.Length;
                }

                if (count < min)
                    return ParseResult.Fail();

                return ParseResult.Create(input, offset);
            };
        }

        private Func<ReadOnlyMemory<char>, ParseResult?> Choice(params Func<ReadOnlyMemory<char>, ParseResult?>[] options)
        {
            return input =>
            {
                foreach (var option in options)
                {
                    var result = option(input);
                    if (result != null)
                        return result;
                }

                return ParseResult.Fail();
            };
        }

        private Func<ReadOnlyMemory<char>, ParseResult?> CharacterRange(char begin, char inclusiveEnd)
        {
            return input =>
            {
                if (input.Length == 0)
                    return ParseResult.Fail();
                var span = input.Span;
                if (span[0] >= begin && span[0] <= inclusiveEnd)
                    return ParseResult.Create(input, 1);

                return ParseResult.Fail();
            };
        }

        private Func<ReadOnlyMemory<char>, ParseResult?> ExactString(string test, bool caseInsensitive)
        {
            var comparison = caseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
            return input =>
            {
                var inputSpan = input.Span;
                var testSpan = test.AsSpan();
                if (inputSpan.StartsWith(testSpan, comparison))
                    return ParseResult.Create(input, testSpan.Length);

                return ParseResult.Fail();
            };
        }

        private Func<ReadOnlyMemory<char>, ParseResult?> Regex(string test, RegexOptions options)
        {
            var regex = new Regex($"^{test}", options);
            return input =>
            {
                var inputSpan = input.Span;
                var match = regex.Match(input.ToString()); // TODO: EnumerateMatches in .NET 7
                if (!match.Success)
                    return ParseResult.Fail();
                return ParseResult.Create(input, match.Length);
            };
        }

        private Func<ReadOnlyMemory<char>, ParseResult?> AnyChar()
        {
            return input =>
            {
                if (input.Length == 0)
                    return ParseResult.Fail();
                return ParseResult.Create(input, 1);
            };
        }

        public readonly struct ParseResult
        {
            public readonly ReadOnlyMemory<char> Memory { get; init; }

            public static ParseResult? Fail() => null;
            public static ParseResult Create(ReadOnlyMemory<char> memory, int length) => new() { Memory = memory.Slice(0, length) };
            public static ParseResult Empty(ReadOnlyMemory<char> memory) => Create(memory, 0);
        }

        public readonly struct ParseResult<T>
        {
            public readonly ReadOnlyMemory<char> Memory { get; init; }
            public readonly T Value { get; init; }
        }

        public readonly struct Unit { }
    }
}
