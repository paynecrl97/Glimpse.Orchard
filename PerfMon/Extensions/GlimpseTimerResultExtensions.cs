using System;
using Glimpse.Orchard.PerfMon.Models;

namespace Glimpse.Orchard.PerfMon.Extensions
{
    public static class GlimpseTimerResultExtensions
    {
        public static TimerResult ToGenericTimerResult(this Core.Extensibility.TimerResult glimpseResult) {
            if (glimpseResult == null) {
                return  new TimerResult();
            }

            return new TimerResult {
                Duration = glimpseResult.Duration,
                Offset = glimpseResult.Offset,
                StartTime = glimpseResult.StartTime
            };
        }

        public static T AsTimedMessage<T>(this T message, TimerResult result) where T : ITimedPerfMonMessage
        {
            message.Duration = result.Duration;
            message.Offset = result.Offset;
            message.StartTime = result.StartTime;

            return message;
        }
    }
}