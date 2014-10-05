using System;
using Glimpse.Orchard.PerfMon.Models;

namespace Glimpse.Orchard.PerfMon.Services {
    public class DefaultPerformanceMonitor : IPerformanceMonitor {
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

        public void PublishTimedAction(Action action) {
            action();
        }

        public void PublishTimedAction<T>(Action action, Func<T> messageFactory) where T : Models.ITimedPerfMonMessage
        {
            action();
        }

        public T PublishTimedAction<T>(Func<T> action) {
            return action();
        }

        public T PublishTimedAction<T, TMessage>(Func<T> action, Func<T, TMessage> messageFactory) where TMessage : Models.ITimedPerfMonMessage
        {
            return action();
        }

        public void PublishMessage(object message) {}
    }
}