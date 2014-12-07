using System.Collections.Generic;
using Glimpse.Orchard.PerfMon.Extensions;
using Glimpse.Orchard.PerfMon.Services;
using Glimpse.Orchard.Tabs.Parts;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Drivers.Coordinators;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;

namespace Glimpse.Orchard.AlternateImplementations {
    /// <summary>
    /// This component coordinates how parts are taking part in the rendering when some content needs to be rendered.
    /// It will dispatch BuildDisplay/BuildEditor to all <see cref="IContentPartDriver"/> implementations.
    /// </summary>
    [OrchardSuppressDependency("Orchard.ContentManagement.Drivers.Coordinators.ContentPartDriverCoordinator")]
    public class GlimpseContentPartDriverCoordinator : ContentPartDriverCoordinator
    {
        private readonly IEnumerable<IContentPartDriver> _drivers;
        private readonly IPerformanceMonitor _performanceMonitor;

        public GlimpseContentPartDriverCoordinator(IEnumerable<IContentPartDriver> drivers, IContentDefinitionManager contentDefinitionManager,  IPerformanceMonitor performanceMonitor)
            : base(drivers, contentDefinitionManager)
        {
            _drivers = drivers;
            _performanceMonitor = performanceMonitor;
        }

        public override void BuildDisplay(BuildDisplayContext context) {
            _drivers.Invoke(driver => {
                var timedResult = _performanceMonitor.Time(() => {
                    var result = driver.BuildDisplay(context);
                    var publish = result != null;
                    if (publish) {
                        result.Apply(context);
                    }

                    return publish;
                });

                if (timedResult.ActionResult)
                {
                    _performanceMonitor.PublishMessage(new PartMessage
                    {
                        Name = context.ContentItem.ContentType,
                    }.AsTimedMessage(timedResult.TimerResult));
                }
            }, Logger);
        }
    }
}