using Glimpse.Core.Extensibility;
using Orchard;

namespace Glimpse.Orchard.MessageBrokers
{
    public interface IPerformanceMessageBroker : IMessageBroker, IDependency
    {}
}