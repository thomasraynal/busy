using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Routable : Attribute
    {
    }
}
