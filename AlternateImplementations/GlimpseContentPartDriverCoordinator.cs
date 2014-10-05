using System.Collections.Generic;
using System.Linq;
using Glimpse.Orchard.Models;
using Glimpse.Orchard.Services;
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
        private readonly IGlimpseService _glimpseService;

        public GlimpseContentPartDriverCoordinator(IEnumerable<IContentPartDriver> drivers, IContentDefinitionManager contentDefinitionManager, IGlimpseService glimpseService)
            : base(drivers, contentDefinitionManager)
        {
            _drivers = drivers;
            _glimpseService = glimpseService;
        }

        public override void BuildDisplay(BuildDisplayContext context) {
            _drivers.Invoke(driver => {
                var publish = false;
                var duration = _glimpseService.Time(() => {
                    var result = driver.BuildDisplay(context);
                    publish = result != null;
                    if (publish)
                        result.Apply(context);
                });

                if (publish) {
                    _glimpseService.MessageBroker.Publish(new PartMessage {
                        Duration = duration.Duration,
                        EventCategory = TimelineCategories.Parts,
                        EventName = "Build Display : " + driver.GetPartInfo().First().PartName,
                        EventSubText = context.ContentItem.ContentType,
                        Name = context.ContentItem.ContentType,
                        Offset = duration.Offset,
                        StartTime = duration.StartTime
                    });
                }
            }, Logger);
        }

        public override void BuildEditor(BuildEditorContext context) {
            _drivers.Invoke(driver =>
            {
                var publish = false;
                var duration = _glimpseService.Time(() => {
                    var result = driver.BuildEditor(context);
                    publish = result != null;
                    if (publish)
                        result.Apply(context);
                });

                if (publish)
                {
                    _glimpseService.MessageBroker.Publish(new PartMessage
                    {
                        Duration = duration.Duration,
                        EventCategory = TimelineCategories.Parts,
                        EventName = "Build Editor : " + driver.GetPartInfo().First().PartName,
                        EventSubText = context.ContentItem.ContentType,
                        Name = context.ContentItem.ContentType,
                        Offset = duration.Offset,
                        StartTime = duration.StartTime
                    });
                }
            }, Logger);
        }

        public override void UpdateEditor(UpdateEditorContext context) {
            _drivers.Invoke(driver =>
            {
                var publish = false;
                var duration = _glimpseService.Time(() =>{
                    var result = driver.UpdateEditor(context);
                    publish = result != null;
                    if (publish)
                        result.Apply(context);
                });

                if (publish)
                {
                    _glimpseService.MessageBroker.Publish(new PartMessage
                    {
                        Duration = duration.Duration,
                        EventCategory = TimelineCategories.Parts,
                        EventName = "Update Editor : " + driver.GetPartInfo().First().PartName,
                        EventSubText = context.ContentItem.ContentType,
                        Name = context.ContentItem.ContentType,
                        Offset = duration.Offset,
                        StartTime = duration.StartTime
                    });
                }
            }, Logger);
        }
    }
}