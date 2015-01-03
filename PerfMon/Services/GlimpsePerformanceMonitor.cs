using System;
using System.Web;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Orchard.Extensions;
using Glimpse.Orchard.Models;
using Glimpse.Orchard.PerfMon.Extensions;
using Glimpse.Orchard.PerfMon.Models;
using Glimpse.Orchard.Tabs.Layers;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;
using TimerResult = Glimpse.Orchard.PerfMon.Models.TimerResult;

namespace Glimpse.Orchard.PerfMon.Services
{
    [OrchardSuppressDependency("Glimpse.Orchard.PerfMon.Services.DefaultPerformanceMonitor")]
    public class GlimpsePerformanceMonitor : DefaultPerformanceMonitor, IPerformanceMonitor
    {
        private readonly LazyField<IMessageBroker> _messageBroker;

        public GlimpsePerformanceMonitor()
        {
            _messageBroker = new LazyField<IMessageBroker>();

            _messageBroker.Loader(GetMessageBroker);
        }

        public new TimerResult Time(Action action)
        {
            var executionTimer = GetTimer();

            if (executionTimer == null)
            {
                return base.Time(action);
            }

            return executionTimer.Time(action).ToGenericTimerResult();
        }

        public new TimedActionResult<T> Time<T>(Func<T> action)
        {
            var result = default(T);

            var executionTimer = GetTimer();

            if (executionTimer == null)
            {
                return base.Time(action);
            }

            var duration = executionTimer.Time(() => { result = action(); }).ToGenericTimerResult();

            return new TimedActionResult<T>
            {
                ActionResult = result,
                TimerResult = duration
            };
        }

        public new TimerResult PublishTimedAction(Action action, PerfmonCategory category, string eventName, string eventSubText = null)
        {
            var timedResult = Time(action);
            PublishMessage(new TimelineMessage { EventName = eventName, EventCategory = category.ToGlimpseTimelineCategoryItem(), EventSubText = eventSubText }.AsTimedMessage(timedResult));

            return timedResult;
        }

        public new TimerResult PublishTimedAction<T>(Action action, Func<T> messageFactory, PerfmonCategory category, string eventName, string eventSubText = null)
        {
            var timedResult = PublishTimedAction(action, category, eventName, eventSubText);
            PublishMessage(messageFactory());

            return timedResult;
        }

        public new TimedActionResult<T> PublishTimedAction<T>(Func<T> action, PerfmonCategory category, string eventName, string eventSubText = null)
        {
            var timedResult = Time(action);
            PublishMessage(new TimelineMessage { EventName = eventName, EventCategory = category.ToGlimpseTimelineCategoryItem(), EventSubText = eventSubText }.AsTimedMessage(timedResult.TimerResult));

            return timedResult;
        }

        public new TimedActionResult<T> PublishTimedAction<T>(Func<T> action, PerfmonCategory category, Func<T, string> eventNameFactory, Func<T, string> eventSubTextFactory = null)
        {
            var timedResult = Time(action);

            string eventSubText = null;
            if (eventSubTextFactory != null)
            {
                eventSubText = eventSubTextFactory(timedResult.ActionResult);
            }

            PublishMessage(new TimelineMessage { EventName = eventNameFactory(timedResult.ActionResult), EventCategory = category.ToGlimpseTimelineCategoryItem(), EventSubText = eventSubText }.AsTimedMessage(timedResult.TimerResult));

            return timedResult;
        }

        public new TimedActionResult<T> PublishTimedAction<T, TMessage>(Func<T> action, Func<T, TimerResult, TMessage> messageFactory, PerfmonCategory category, string eventName, string eventSubText = null)
        {
            var actionResult = PublishTimedAction(action, category, eventName, eventSubText);
            PublishMessage(messageFactory(actionResult.ActionResult, actionResult.TimerResult));

            return actionResult;
        }

        public new TimedActionResult<T> PublishTimedAction<T, TMessage>(Func<T> action, Func<T, TimerResult, TMessage> messageFactory, PerfmonCategory category, Func<T, string> eventNameFactory, Func<T, string> eventSubTextFactory = null)
        {
            var actionResult = PublishTimedAction(action, category, eventNameFactory, eventSubTextFactory);
            PublishMessage(messageFactory(actionResult.ActionResult, actionResult.TimerResult));

            return actionResult;
        }

        public new void PublishMessage<T>(T message)
        {
            _messageBroker.Value.Publish(message);
        }

        private IExecutionTimer GetTimer()
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                return null;
            }

            return ((GlimpseRuntime)context.Application.Get("__GlimpseRuntime")).Configuration.TimerStrategy.Invoke();
        }

        private IMessageBroker GetMessageBroker(IMessageBroker messageBroker)
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                return new NullMessageBroker();
            }

            return ((GlimpseRuntime)context.Application.Get("__GlimpseRuntime")).Configuration.MessageBroker;
        }

        public class NullMessageBroker : IMessageBroker
        {
            public void Publish<T>(T message) { }

            public Guid Subscribe<T>(Action<T> action)
            {
                return Guid.NewGuid();
            }

            public void Unsubscribe<T>(Guid subscriptionId) { }
        }
    }
}