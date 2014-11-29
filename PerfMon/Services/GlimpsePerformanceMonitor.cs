using System;
using System.Web;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Core.Message;
using Glimpse.Orchard.PerfMon.Extensions;
using Glimpse.Orchard.PerfMon.Models;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;
using TimerResult = Glimpse.Orchard.PerfMon.Models.TimerResult;

namespace Glimpse.Orchard.PerfMon.Services {
    [OrchardSuppressDependency("Glimpse.Orchard.PerfMon.Services.DefaultPerformanceMonitor")]
    public class GlimpsePerformanceMonitor : DefaultPerformanceMonitor, IPerformanceMonitor {
        private readonly LazyField<IMessageBroker> _messageBroker;

        public GlimpsePerformanceMonitor() {
            _messageBroker = new LazyField<IMessageBroker>();

            _messageBroker.Loader(GetMessageBroker);
        }

        public new TimerResult Time(Action action)
        {
            var executionTimer = GetTimer();

            if (executionTimer == null){
                return base.Time(action);
            }

            return executionTimer.Time(action).ToGenericTimerResult();
        }

        public new TimedActionResult<T> Time<T>(Func<T> action)
        {
            var result = default(T);

            var executionTimer = GetTimer();

            if (executionTimer == null) {
                return base.Time(action);
            }

            var duration = executionTimer.Time(()=> { result = action(); }).ToGenericTimerResult();

            return new TimedActionResult<T> {
                ActionResult = result,
                TimerResult = duration
            };
        }

        public new void PublishTimedAction(Action action)
        {
            PublishTimedAction(action, () => new GlimpseTimedMessage());
        }

        public new void PublishTimedAction<T>(Action action, Func<T> messageFactory) where T : ITimedPerfMonMessage
        {
            var timedResult = Time(action);
            PublishMessage(messageFactory().AsTimedMessage(timedResult));
        }

        public new T PublishTimedAction<T>(Func<T> action)
        {
            return PublishTimedAction(action, t => new GlimpseTimedMessage());
        }

        public new T PublishTimedAction<T, TMessage>(Func<T> action, Func<T, TMessage> messageFactory) where TMessage : ITimedPerfMonMessage
        {
            var timedResult = Time(action);
            PublishMessage(messageFactory(timedResult.ActionResult).AsTimedMessage(timedResult.TimerResult));
            return timedResult.ActionResult;
        }

        public new void PublishMessage(object message)
        {
            _messageBroker.Value.Publish(message);
        }

        private IExecutionTimer GetTimer() {
            var context = HttpContext.Current;
            if (context == null) {
                return null;
            }

            return ((GlimpseRuntime)context.Application.Get("__GlimpseRuntime")).Configuration.TimerStrategy.Invoke();
        }

        private IMessageBroker GetMessageBroker(IMessageBroker messageBroker){
            var context = HttpContext.Current;
            if (context == null){
                return new NullMessageBroker();
            }

            return ((GlimpseRuntime)context.Application.Get("__GlimpseRuntime")).Configuration.MessageBroker;
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

        public class NullMessageBroker : IMessageBroker  {
            public void Publish<T>(T message) {}

            public Guid Subscribe<T>(Action<T> action) {
                return Guid.NewGuid();
            }

            public void Unsubscribe<T>(Guid subscriptionId) {}
        }
    }
}