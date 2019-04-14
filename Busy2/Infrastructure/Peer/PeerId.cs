using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public readonly struct PeerId
    {
        private readonly string _value;

        public PeerId(string value) : this()
        {
            _value = value;
        }

        public bool Equals(PeerId other) => string.Equals(_value, other._value);
        public override bool Equals(object obj) => obj is PeerId && Equals((PeerId)obj);

        public override int GetHashCode() => _value?.GetHashCode() ?? 0;

        public static bool operator ==(PeerId left, PeerId right) => left.Equals(right);
        public static bool operator !=(PeerId left, PeerId right) => !left.Equals(right);

        public override string ToString() => _value ?? string.Empty;

    }
}
