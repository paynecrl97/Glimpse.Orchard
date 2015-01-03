using System;

namespace Glimpse.Orchard.Extensions {
    public static class TimespanExtensions {
        public static string ToTimingString(this TimeSpan timespan) {
            return string.Format("{0} ms", Math.Round(timespan.TotalMilliseconds, 2));
        }
    }
}