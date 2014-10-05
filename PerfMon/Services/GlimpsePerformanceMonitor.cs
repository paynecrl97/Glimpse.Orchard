using System;
using System.Web;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Core.Message;
using Glimpse.Orchard.PerfMon.Extensions;
using Glimpse.Orchard.PerfMon.Models;
using TimerResult = Glimpse.Orchard.PerfMon.Models.TimerResult;

namespace Glimpse.Orchard.PerfMon.Services {
    public class GlimpsePerformanceMonitor : IPerformanceMonitor {
        public TimerResult Time(Action action)
        {
            var executionTimer = GetTimer();
            return executionTimer.Time(action).ToGenericTimerResult();
        }

        public TimedActionResult<T> Time<T>(Func<T> action) {
            var result = default(T);

            var executionTimer = GetTimer();
            var duration = executionTimer.Time(()=> { result = action(); }).ToGenericTimerResult();

            return new TimedActionResult<T> {
                ActionResult = result,
                TimerResult = duration
            };
        }

        public void PublishTimedAction(Action action)
        {
            PublishTimedAction(action, () => new GlimpseTimedMessage());
        }

        public void PublishTimedAction<T>(Action action, Func<T> messageFactory) where T : ITimedPerfMonMessage
        {
            var timedResult = Time(action);
            PublishMessage(messageFactory().AsTimedMessage(timedResult));
        }

        public T PublishTimedAction<T>(Func<T> action) {
            return PublishTimedAction(action, t => new GlimpseTimedMessage());
        }

        public T PublishTimedAction<T, TMessage>(Func<T> action, Func<T, TMessage> messageFactory) where TMessage : ITimedPerfMonMessage
        {
            var timedResult = Time(action);
            PublishMessage(messageFactory(timedResult.ActionResult).AsTimedMessage(timedResult.TimerResult));
            return timedResult.ActionResult;
        }

        public void PublishMessage(object message)
        {
            var messageBroker = GetMessageBroker();

            messageBroker.Publish(message);
        }

        private IExecutionTimer GetTimer() {
            return ((GlimpseRuntime)HttpContext.Current.Application.Get("__GlimpseRuntime")).Configuration.TimerStrategy.Invoke();
        }

        private IMessageBroker GetMessageBroker() {
            return ((GlimpseRuntime)HttpContext.Current.Application.Get("__GlimpseRuntime")).Configuration.MessageBroker;
        }

        private class GlimpseTimedMessage : ITimedMessage, ITimedPerfMonMessage
        {
            public GlimpseTimedMessage()
            {
                Id = Guid.NewGuid();
            }

            public Guid Id { get; private set; }
            public TimeSpan Offset { get; set; }
            public TimeSpan Duration { get; set; }
            public DateTime StartTime { get; set; }
        }
    }
}