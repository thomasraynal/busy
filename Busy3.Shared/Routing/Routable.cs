using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Routable : Attribute
    {
    }
}
