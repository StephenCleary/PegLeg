using System;
using System.Collections.Generic;
using System.Text;

namespace PegLeg
{
    public sealed class PegParser
    {
        public ParseResult? TryParse(ReadOnlyMemory<char> input)
        {
            return Identifier(input);
        }

        private ParseResult? Identifier(ReadOnlyMemory<char> input)
        {
            // (LowercaseLetter / UppercaseLetter / Underscore) (LowercaseLetter / UppercaseLetter / Underscore / Digit)*
            var firstLetter = Choice(LowercaseLetter, UppercaseLetter, Underscore);
            var otherLetters = Star(Choice(LowercaseLetter, UppercaseLetter, Underscore, Digit));
            return Sequence(firstLetter, otherLetters)(input);
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
                        return null;
                    offset += childResult.Value.Memory.Length;
                }

                return new ParseResult { Memory = input.Slice(0, offset) };
            };
        }

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

                return new ParseResult { Memory = input.Slice(0, offset) };
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

                return null;
            };
        }

        private ParseResult? LowercaseLetter(ReadOnlyMemory<char> input) => CharacterRange(input, 'a', 'z');
        private ParseResult? UppercaseLetter(ReadOnlyMemory<char> input) => CharacterRange(input, 'A', 'Z');
        private ParseResult? Digit(ReadOnlyMemory<char> input) => CharacterRange(input, '0', '9');

        private ParseResult? CharacterRange(ReadOnlyMemory<char> input, char begin, char inclusiveEnd)
        {
            if (input.Length == 0)
                return null;
            var span = input.Span;
            if (span[0] >= begin && span[0] <= inclusiveEnd)
                return new ParseResult() { Memory = input.Slice(0, 1) };

            return null;
        }

        private ParseResult? Underscore(ReadOnlyMemory<char> input) => ExactString(input, "_", caseInsensitive: false);

        private ParseResult? ExactString(ReadOnlyMemory<char> input, string test, bool caseInsensitive)
        {
            if (input.Length == 0)
                return null;
            var span = input.Span;
            if (span.StartsWith(test.AsSpan(), caseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
                return new ParseResult() { Memory = input.Slice(0, test.Length) };

            return null;
        }

        public readonly struct ParseResult
        {
            public readonly ReadOnlyMemory<char> Memory { get; init; }
        }
    }
}
