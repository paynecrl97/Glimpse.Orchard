using System;
using Glimpse.Orchard.Models;
using Glimpse.Orchard.PerfMon.Models;

namespace Glimpse.Orchard.PerfMon.Services {
    public class DefaultPerformanceMonitor : IPerformanceMonitor 
    {
        public TimerResult Time(Action action) {
            action();

            return new TimerResult();
        }

        public TimedActionResult<T> Time<T>(Func<T> action) {
            return new TimedActionResult<T> {
                TimerResult = new TimerResult(),
                ActionResult = action()
            };
        }

        public TimerResult PublishTimedAction(Action action, PerfmonCategory category, string eventName, string eventSubText = null)
        {
            return Time(action);
        }

        public TimerResult PublishTimedAction<T>(Action action, Func<T> messageFactory, PerfmonCategory category, string eventName, string eventSubText = null)
        {
            return Time(action);
        }

        public TimedActionResult<T> PublishTimedAction<T>(Func<T> action, PerfmonCategory category, string eventName, string eventSubText = null)
        {
            return Time(action);
        }

        public TimedActionResult<T> PublishTimedAction<T>(Func<T> action, PerfmonCategory category, Func<T, string> eventNameFactory, Func<T, string> eventSubTextFactory = null)
        {
            return Time(action);
        }

        public TimedActionResult<T> PublishTimedAction<T, TMessage>(Func<T> action, Func<T, TimerResult, TMessage> messageFactory, PerfmonCategory category, string eventName, string eventSubText = null)
        {
            return Time(action);
        }

        public TimedActionResult<T> PublishTimedAction<T, TMessage>(Func<T> action, Func<T,TimerResult, TMessage> messageFactory, PerfmonCategory category, Func<T, string> eventNameFactory, Func<T, string> eventSubTextFactory = null)
        {
            return Time(action);
        }

        public void PublishMessage<T>(T message) {}
    }
}