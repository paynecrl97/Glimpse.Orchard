

////todo: this class currently records every field available- even if the field is not actually bound to the content. figure out how to stop this from happening


//using System.Collections.Generic;
//using System.Linq;
//using Glimpse.Orchard.Models;
//using Glimpse.Orchard.Services;
//using Glimpse.Orchard.Tabs.Fields;
//using JetBrains.Annotations;
//using Orchard;
//using Orchard.ContentManagement.Drivers;
//using Orchard.ContentManagement.Drivers.Coordinators;
//using Orchard.ContentManagement.FieldStorage;
//using Orchard.ContentManagement.Handlers;
//using Orchard.Environment.Extensions;

//namespace Glimpse.Orchard.AlternateImplementations
//{
//    [UsedImplicitly]
//    [OrchardSuppressDependency("Orchard.ContentManagement.Drivers.Coordinators.ContentFieldDriverCoordinator")]
//    public class GlimpseContentFieldDriverCoordinator : ContentFieldDriverCoordinator
//    {
//        private readonly IEnumerable<IContentFieldDriver> _drivers;
//        private readonly IGlimpseService _glimpseService;

//        public GlimpseContentFieldDriverCoordinator(
//            IEnumerable<IContentFieldDriver> drivers,
//            IFieldStorageProviderSelector fieldStorageProviderSelector,
//            IEnumerable<IFieldStorageEvents> fieldStorageEvents,
//            IGlimpseService glimpseService)
//            : base(drivers, fieldStorageProviderSelector, fieldStorageEvents)
//        {
//            _drivers = drivers;
//            _glimpseService = glimpseService;
//        }

//        public override void BuildDisplay(BuildDisplayContext context)
//        {
//            _drivers.Invoke(driver =>
//            {
//                context.Logger = Logger;
//                var publish = false;
//                var duration = _glimpseService.Time(() =>
//                {
//                    var result = driver.BuildDisplayShape(context);
//                    publish = result != null;
//                    if (publish)
//                        result.Apply(context);
//                });

//                if (publish)
//                {
//                    _glimpseService.MessageBroker.Publish(new FieldMessage
//                    {
//                        Duration = duration.Duration,
//                        EventCategory = TimelineCategories.Fields,
//                        EventName = "Build Display : " + driver.GetFieldInfo().First().FieldTypeName,
//                        EventSubText = context.ContentItem.ContentType,
//                        Name = context.ContentItem.ContentType,
//                        Offset = duration.Offset,
//                        StartTime = duration.StartTime
//                    });
//                }
//            }, Logger);
//        }

//        public override void BuildEditor(BuildEditorContext context)
//        {
//            _drivers.Invoke(driver =>
//            {
//                context.Logger = Logger;
//                var publish = false;
//                var duration = _glimpseService.Time(() =>
//                {
//                    var result = driver.BuildEditorShape(context);
//                    publish = result != null;
//                    if (publish)
//                        result.Apply(context);
//                });

//                if (publish)
//                {
//                    _glimpseService.MessageBroker.Publish(new FieldMessage
//                    {
//                        Duration = duration.Duration,
//                        EventCategory = TimelineCategories.Fields,
//                        EventName = "Build Editor : " + driver.GetFieldInfo().First().FieldTypeName,
//                        EventSubText = context.ContentItem.ContentType,
//                        Name = context.ContentItem.ContentType,
//                        Offset = duration.Offset,
//                        StartTime = duration.StartTime
//                    });
//                }
//            }, Logger);
//        }

//        public override void UpdateEditor(UpdateEditorContext context)
//        {
//            _drivers.Invoke(driver =>
//            {
//                context.Logger = Logger;
//                var publish = false;
//                var duration = _glimpseService.Time(() =>
//                {
//                    var result = driver.UpdateEditorShape(context);
//                    publish = result != null;
//                    if (publish)
//                        result.Apply(context);
//                });

//                if (publish)
//                {
//                    _glimpseService.MessageBroker.Publish(new FieldMessage
//                    {
//                        Duration = duration.Duration,
//                        EventCategory = TimelineCategories.Fields,
//                        EventName = "Update Editor : " + driver.GetFieldInfo().First().FieldTypeName,
//                        EventSubText = context.ContentItem.ContentType,
//                        Name = context.ContentItem.ContentType,
//                        Offset = duration.Offset,
//                        StartTime = duration.StartTime
//                    });
//                }
//            }, Logger);
//        }
//    }
//}