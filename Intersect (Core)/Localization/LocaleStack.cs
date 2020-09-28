using System;
using System.Globalization;
using System.Threading;

namespace Intersect.Localization
{
    public sealed class LocaleStack : IDisposable
    {
        public static CultureInfo CurrentLocale => Thread.CurrentThread.CurrentCulture;

        public static IDisposable Push(CultureInfo cultureInfo)
        {
            var cultureInfoStack = new LocaleStack();

            Thread.CurrentThread.CurrentCulture = cultureInfo;

            return cultureInfoStack;
        }

        private readonly CultureInfo originalCultureInfo;

        private LocaleStack()
        {
            originalCultureInfo = Thread.CurrentThread.CurrentCulture;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = originalCultureInfo;
        }
    }
}
