using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class RoutingPositionAttribute : Attribute
    {
        public int Position { get; }

        public RoutingPositionAttribute(int position)
        {
            Position = position;
        }
    }
}
