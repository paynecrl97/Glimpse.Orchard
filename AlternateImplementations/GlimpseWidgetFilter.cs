using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Glimpse.Core.Framework;
using Glimpse.Core.Message;
using Glimpse.Orchard.Models;
using Glimpse.Orchard.Services;
using Glimpse.Orchard.Tabs.Layers;
using Glimpse.Orchard.Tabs.Widgets;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Settings.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;
using ILogger = Orchard.Logging.ILogger;
using NullLogger = Orchard.Logging.NullLogger;

namespace Glimpse.Orchard.AlternateImplementations
{
    [OrchardSuppressDependency("Orchard.Widgets.Filters.WidgetFilter")]
    public class GlimpseWidgetFilter : FilterProvider, IResultFilter
    {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRuleManager _ruleManager;
        private readonly IWidgetsService _widgetsService;
        private readonly IOrchardServices _orchardServices;
        private readonly IGlimpseService _glimpseService;

        public GlimpseWidgetFilter(
            IWorkContextAccessor workContextAccessor,
            IRuleManager ruleManager,
            IWidgetsService widgetsService,
            IOrchardServices orchardServices,
            IGlimpseService glimpseService)
        {
            _workContextAccessor = workContextAccessor;
            _ruleManager = ruleManager;
            _widgetsService = widgetsService;
            _orchardServices = orchardServices;
            _glimpseService = glimpseService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; private set; }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // layers and widgets should only run on a full view rendering result
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult == null)
                return;

            var workContext = _workContextAccessor.GetContext(filterContext);

            if (workContext == null ||
                workContext.Layout == null ||
                workContext.CurrentSite == null ||
                AdminFilter.IsApplied(filterContext.RequestContext) ||
                !ThemeFilter.IsApplied(filterContext.RequestContext))
            {
                return;
            }

            // Once the Rule Engine is done:
            // Get Layers and filter by zone and rule
            IEnumerable<LayerPart> activeLayers = _orchardServices.ContentManager.Query<LayerPart, LayerPartRecord>().List();

            var activeLayerIds = new List<int>();
            foreach (var activeLayer in activeLayers)
            {
                // ignore the rule if it fails to execute
                try
                {
                    var layerRuleMatches = true;
                    var currentLayer = activeLayer;
                    var duration = _glimpseService.Time(() => { layerRuleMatches = _ruleManager.Matches(currentLayer.Record.LayerRule); });


                    _glimpseService.MessageBroker.Publish(new LayerMessage
                    {
                        Active = layerRuleMatches,
                        Name = activeLayer.Record.Name,
                        Rule = activeLayer.Record.LayerRule,//
                        Duration = duration.Duration,
                        EventCategory = TimelineCategories.Layers,
                        EventName = "Layer Evaluation",
                        EventSubText = activeLayer.Record.Name,
                        Offset = duration.Offset,
                        StartTime = duration.StartTime
                    });

                    if (layerRuleMatches)
                    {
                        activeLayerIds.Add(activeLayer.ContentItem.Id);
                    }
                }
                catch (Exception e)
                {
                    Logger.Warning(e, T("An error occured during layer evaluation on: {0}", activeLayer.Name).Text);
                }
            }

            IEnumerable<WidgetPart> widgetParts = _widgetsService.GetWidgets(layerIds: activeLayerIds.ToArray());

            // Build and add shape to zone.
            var zones = workContext.Layout.Zones;
            var defaultCulture = workContext.CurrentSite.As<SiteSettingsPart>().SiteCulture;
            var currentCulture = workContext.CurrentCulture;

            foreach (var widgetPart in widgetParts)
            {
                var commonPart = widgetPart.As<ICommonPart>();
                if (commonPart == null || commonPart.Container == null)
                {
                    Logger.Warning("The widget '{0}' is has no assigned layer or the layer does not exist.", widgetPart.Title);
                    continue;
                }

                // ignore widget for different cultures
                var localizablePart = widgetPart.As<ILocalizableAspect>();
                if (localizablePart != null)
                {
                    // if localized culture is null then show if current culture is the default
                    // this allows a user to show a content item for the default culture only
                    if (localizablePart.Culture == null && defaultCulture != currentCulture)
                    {
                        continue;
                    }

                    // if culture is set, show only if current culture is the same
                    if (localizablePart.Culture != null && localizablePart.Culture != currentCulture)
                    {
                        continue;
                    }
                }

                // check permissions
                if (!_orchardServices.Authorizer.Authorize(global::Orchard.Core.Contents.Permissions.ViewContent, widgetPart))
                {
                    continue;
                }

                dynamic widgetShape = null;
                var currentWidgetPart = widgetPart;

                var duration = _glimpseService.Time(() =>
                {
                    widgetShape = _orchardServices.ContentManager.BuildDisplay(currentWidgetPart);
                });

                _glimpseService.MessageBroker.Publish(new WidgetMessage
                {
                    Name = currentWidgetPart.Name,
                    Type = currentWidgetPart.ContentItem.ContentType,
                    Zone = currentWidgetPart.Zone,
                    Duration = duration.Duration,
                    EventCategory = TimelineCategories.Widgets,
                    EventName = "Widget Build Display",
                    EventSubText = currentWidgetPart.ContentItem.ContentType,
                    Offset = duration.Offset,
                    StartTime = duration.StartTime
                });

                zones[widgetPart.Record.Zone].Add(widgetShape, widgetPart.Record.Position);
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}
