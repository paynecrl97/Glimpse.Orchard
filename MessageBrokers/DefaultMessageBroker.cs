using System;
using System.Web;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Orchard.Core.Common.Utilities;

namespace Glimpse.Orchard.MessageBrokers {
    public class DefaultMessageBroker : IPerformanceMessageBroker 
    {
        private readonly LazyField<IMessageBroker> _messageBroker;

        public DefaultMessageBroker() {
            _messageBroker = new LazyField<IMessageBroker>();

            _messageBroker.Loader(() => {
                var context = HttpContext.Current;
                if (context == null)
                {
                    return new NullMessageBroker();
                }

                return ((GlimpseRuntime)context.Application.Get("__GlimpseRuntime")).Configuration.MessageBroker;
            });
        }

        public void Publish<T>(T message) {
            _messageBroker.Value.Publish(message);
        }

        public Guid Subscribe<T>(Action<T> action)
        {
            return  _messageBroker.Value.Subscribe(action);
        }

        public void Unsubscribe<T>(Guid subscriptionId)
        {
            _messageBroker.Value.Unsubscribe<T>(subscriptionId);
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