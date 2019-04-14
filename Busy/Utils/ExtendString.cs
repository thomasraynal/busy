using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    internal static class ExtendString
    {
        public static string Qualifier(this string input)
        {
            if (input == null)
                return null;

            var lastDotIndex = input.LastIndexOf('.');
            if (lastDotIndex == -1)
                return input;

            return input.Substring(0, lastDotIndex);
        }
    }
}
