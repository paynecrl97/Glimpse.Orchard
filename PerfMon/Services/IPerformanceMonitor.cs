using System;
using Glimpse.Orchard.PerfMon.Models;
using Orchard;

namespace Glimpse.Orchard.PerfMon.Services
{
    public interface IPerformanceMonitor : IDependency
    {
        TimerResult Time(Action action);
        TimedActionResult<T> Time<T>(Func<T> action);
        void PublishTimedAction(Action action);
        void PublishTimedAction<T>(Action action, Func<T> messageFactory) where T : ITimedPerfMonMessage;
        T PublishTimedAction<T>(Func<T> action);
        T PublishTimedAction<T, TMessage>(Func<T> action, Func<T, TMessage> messageFactory) where TMessage : ITimedPerfMonMessage;
        void PublishMessage(object message);
    }
}