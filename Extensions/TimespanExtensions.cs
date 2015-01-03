using System;

namespace Glimpse.Orchard.Extensions {
    public static class TimespanExtensions
    {
        public static string ToTimingString(this TimeSpan timespan) {
            return timespan.TotalMilliseconds.ToTimingString();
        }
        public static string ToTimingString(this double milliseconds)
        {
            return string.Format("{0} ms", Math.Round(milliseconds, 2));
        }
    }
}