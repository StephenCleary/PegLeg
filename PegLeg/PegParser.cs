using PegLeg.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PegLeg
{
    public sealed class PegParser
    {
        public ParseResult? TryParse(Substring input)
        {
            return Identifier(input);

            ParseResult? Identifier(Substring input)
            {
                // (LowercaseLetter / UppercaseLetter / Underscore) (LowercaseLetter / UppercaseLetter / Underscore / Digit)*
                var firstLetter = Choice(LowercaseLetter, UppercaseLetter, Underscore);
                var otherLetters = Star(Choice(LowercaseLetter, UppercaseLetter, Underscore, Digit));
                return Sequence(firstLetter, otherLetters)(input);
            }

            ParseResult? LowercaseLetter(Substring input) => CharacterRange('a', 'z')(input);
            ParseResult? UppercaseLetter(Substring input) => CharacterRange('A', 'Z')(input);
            ParseResult? Digit(Substring input) => CharacterRange('0', '9')(input);
            ParseResult? Underscore(Substring input) => ExactString("_", caseInsensitive: false)(input);
        }

        private Func<Substring, ParseResult?> PositiveLookahead(Func<Substring, ParseResult?> child)
        {
            return input =>
            {
                var result = child(input);
                if (result == null)
                    return ParseResult.Fail();
                return ParseResult.Empty(input);
            };
        }

        private Func<Substring, ParseResult?> NegativeLookahead(Func<Substring, ParseResult?> child)
        {
            return input =>
            {
                var result = child(input);
                if (result == null)
                    return ParseResult.Empty(input);
                return ParseResult.Fail();
            };
        }

        private Func<Substring, ParseResult?> Sequence(params Func<Substring, ParseResult?>[] children)
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
                    offset += childResult.Value.Substring.Length;
                }

                return ParseResult.Create(input, offset);
            };
        }

        private Func<Substring, ParseResult?> Optional(Func<Substring, ParseResult?> child)
        {
            return input =>
            {
                var result = child(input);
                if (result != null)
                    return result;

                return ParseResult.Empty(input);
            };
        }

        private Func<Substring, ParseResult?> Plus(Func<Substring, ParseResult?> child) => Sequence(child, Star(child));

        private Func<Substring, ParseResult?> Star(Func<Substring, ParseResult?> child)
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
                    offset += childResult.Value.Substring.Length;
                }

                return ParseResult.Create(input, offset);
            };
        }

        private Func<Substring, ParseResult?> Quantify(Func<Substring, ParseResult?> child, int min, int? inclusiveMax, Func<Substring, ParseResult?>? delimiter)
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
                        offset += delimiterResult.Value.Substring.Length;
                    }

                    var childInput = input.Slice(offset);
                    var childResult = child(childInput);
                    if (childResult == null)
                    {
                        offset = offsetBeforeDelimiter;
                        break;
                    }

                    offset += childResult.Value.Substring.Length;
                }

                if (count < min)
                    return ParseResult.Fail();

                return ParseResult.Create(input, offset);
            };
        }

        private Func<Substring, ParseResult?> Choice(params Func<Substring, ParseResult?>[] options)
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

        private Func<Substring, ParseResult?> CharacterRange(char begin, char inclusiveEnd)
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

        private Func<Substring, ParseResult?> ExactString(string test, bool caseInsensitive)
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

        private Func<Substring, ParseResult?> Regex(string test, RegexOptions options)
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

        private Func<Substring, ParseResult?> AnyChar()
        {
            return input =>
            {
                if (input.Length == 0)
                    return ParseResult.Fail();
                return ParseResult.Create(input, 1);
            };
        }

        public readonly struct ParseState
        {
            public readonly ReadOnlyMemory<char> Memory { get; init; }
            public readonly ImmutableDictionary<string, dynamic> State { get; init; }
        }

        public readonly struct ParseResult
        {
            public readonly Substring Substring { get; init; }

            public static ParseResult? Fail() => null;
            public static ParseResult Create(Substring substring, int length) => new() { Substring = substring.Slice(0, length) };
            public static ParseResult Empty(Substring substring) => Create(substring, 0);
        }

        public readonly struct ParseResult<T>
        {
            public readonly ReadOnlyMemory<char> Memory { get; init; }
            public readonly T Value { get; init; }

            public static ParseResult<T>? Fail() => null;
            public static ParseResult<T> Create(ParseState state, int length, Func<ParseState, ReadOnlyMemory<char>, T> getValue)
            {
                var memory = state.Memory.Slice(0, length);
                return new()
                {
                    Memory = memory,
                    Value = getValue(state, memory),
                };
            }
            public static ParseResult<T> Empty(ParseState state, Func<ParseState, ReadOnlyMemory<char>, T> getValue) => Create(state, 0, getValue);
            public static ParseResult<ReadOnlyMemory<char>> Create(ParseState state, int length)
            {
                var memory = state.Memory.Slice(0, length);
                return new()
                {
                    Memory = memory,
                    Value = memory,
                };
            }
            public static ParseResult<ReadOnlyMemory<char>> Empty(ParseState state) => Create(state, 0);
        }

        public readonly struct Unit { }
    }
}
