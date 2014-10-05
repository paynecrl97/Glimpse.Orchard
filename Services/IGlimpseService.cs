using System;
using System.Web;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Orchard;

namespace Glimpse.Orchard.Services
{
    public interface IGlimpseService : IDependency {
        TimerResult Time(Action action);
        IMessageBroker MessageBroker { get; }
    }

    public class GlimpseService : IGlimpseService {
        public TimerResult Time(Action action)
        {
            var executionTimer = ((GlimpseRuntime)HttpContext.Current.Application.Get("__GlimpseRuntime")).Configuration.TimerStrategy.Invoke();
            return executionTimer.Time(action);
        }

        public IMessageBroker MessageBroker { get { return ((GlimpseRuntime) HttpContext.Current.Application.Get("__GlimpseRuntime")).Configuration.MessageBroker; } }
    }
}