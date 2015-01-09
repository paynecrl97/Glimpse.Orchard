using System.Linq;
using System.Web.Mvc;
using Glimpse.Orchard.PerfMon.Services;
using Glimpse.Orchard.Tabs.SiteSettings;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;

namespace Glimpse.Orchard.Filters
{
    [OrchardFeature("Glimpse.Orchard.SiteSettings")]
    public class SiteSettingsFilter : FilterProvider, IResultFilter
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IPerformanceMonitor _performanceMonitor;

        public SiteSettingsFilter(IOrchardServices orchardServices, IPerformanceMonitor performanceMonitor) {
            _orchardServices = orchardServices;
            _performanceMonitor = performanceMonitor;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {}

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            foreach (var sitePart in _orchardServices.WorkContext.CurrentSite.ContentItem.Parts)
            {
                foreach (var property in sitePart.GetType().GetProperties().Where(p=>p.Name!="Id"))//id will always be 1 because this part is bound to the site
                {
                    var propertyType = property.PropertyType;
                    // Supported types (we also know they are not indexed properties).
                    if ((propertyType == typeof(string) || propertyType == typeof(bool) || propertyType == typeof(int)) 
                        && property.CanRead) {
                        var value = property.GetValue(sitePart, null);

                        _performanceMonitor.PublishMessage(new SiteSettingsMessage {
                            Part = sitePart.PartDefinition.Name,
                            Name = property.Name,
                            Value = value
                        });
                    }
                }
            }
        }
    }
}