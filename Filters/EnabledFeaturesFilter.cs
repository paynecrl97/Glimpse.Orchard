using System.Web.Mvc;
using Glimpse.Orchard.PerfMon.Services;
using Glimpse.Orchard.Tabs.EnabledFeatures;
using Orchard.Environment.Extensions;
using Orchard.Environment.Features;
using Orchard.Mvc.Filters;

namespace Glimpse.Orchard.Filters
{
    [OrchardFeature("Glimpse.Orchard.EnabledFeatures")]
    public class EnabledFeaturesFilter : FilterProvider, IResultFilter
    {
        private readonly IFeatureManager _featureManager;
        private readonly IPerformanceMonitor _performanceMonitor;

        public EnabledFeaturesFilter(IFeatureManager featureManager, IPerformanceMonitor performanceMonitor) {
            _featureManager = featureManager;
            _performanceMonitor = performanceMonitor;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {}

        public void OnResultExecuted(ResultExecutedContext filterContext) {
            var enabledFeatures = _featureManager.GetEnabledFeatures();

            foreach (var feature in enabledFeatures) {
                _performanceMonitor.PublishMessage(new EnabledFeatureMessage {
                    Category = feature.Category,
                    Description = feature.Description,
                    Extension = feature.Extension,
                    FeatureId = feature.Id,
                    Name = feature.Name,
                    Priority = feature.Priority
                });
            }
        }
    }
}