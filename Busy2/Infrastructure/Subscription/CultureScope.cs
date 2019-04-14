using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Busy.Infrastructure
{
    internal class CultureScope : IDisposable
    {
        private readonly CultureInfo _culture;
        private readonly CultureInfo _uiCulture;

        public CultureScope(CultureInfo culture)
        {
            _culture = Thread.CurrentThread.CurrentCulture;
            _uiCulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public static CultureScope Invariant()
        {
            return new CultureScope(CultureInfo.InvariantCulture);
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = _culture;
            Thread.CurrentThread.CurrentUICulture = _uiCulture;
        }
    }
}
