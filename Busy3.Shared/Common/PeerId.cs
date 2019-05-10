using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class PeerId
    {
      

        public PeerId(string value)
        {
            Value = value;
        }

        public PeerId()
        {
            Value = string.Empty;
        }

        public string Value { get; set; }

        public bool Equals(PeerId other) => string.Equals(Value, other.Value);
        public override bool Equals(object obj) => obj is PeerId && Equals((PeerId)obj);

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public static bool operator ==(PeerId left, PeerId right) => left.Equals(right);
        public static bool operator !=(PeerId left, PeerId right) => !left.Equals(right);

        public override string ToString() => Value ?? string.Empty;

    }
}
