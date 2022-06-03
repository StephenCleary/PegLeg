using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PegLeg.Runtime
{
    public readonly struct Substring
    {
        private readonly string _value;
        private readonly int _start;
        private readonly int _end;

        // TODO: maybe make internal?
        public Substring(string value, int start, int end) => (_value, _start, _end) = (value, start, end);
        public Substring(string value) => (_value, _start, _end) = (value, 0, value.Length);

        public ReadOnlySpan<char> Span => _value.AsSpan().Slice(_start, Length);
        public int Length => _end - _start;

        // TODO: no boundary checks. OK?
        public Substring Slice(int index, int length) => new (_value, _start + index, _start + index + length);
        public Substring Slice(int index) => new (_value, _start + index, _end - index);

        public override string ToString() => _value.Substring(_start, Length);
    }
}
