using System;
using System.Collections.Generic;
using Autofac;
using Glimpse.Orchard.Models;
using Glimpse.Orchard.PerfMon.Services;
using Glimpse.Orchard.Tabs.ContentManager;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Data.Providers;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Users.Models;
using Orchard.Widgets.Models;

namespace Glimpse.Orchard.AlternateImplementations
{
    [OrchardSuppressDependency("Orchard.ContentManagement.DefaultContentManager")]
    public class GlimpseContentManager : DefaultContentManager, IContentManager
    {
        private readonly IPerformanceMonitor _performanceMonitor;

        public GlimpseContentManager(
            IComponentContext context,
            IRepository<ContentTypeRecord> contentTypeRepository,
            IRepository<ContentItemRecord> contentItemRepository,
            IRepository<ContentItemVersionRecord> contentItemVersionRepository,
            IContentDefinitionManager contentDefinitionManager,
            ICacheManager cacheManager,
            Func<IContentManagerSession> contentManagerSession,
            Lazy<IContentDisplay> contentDisplay,
            Lazy<ISessionLocator> sessionLocator,
            Lazy<IEnumerable<IContentHandler>> handlers,
            Lazy<IEnumerable<IIdentityResolverSelector>> identityResolverSelectors,
            Lazy<IEnumerable<ISqlStatementProvider>> sqlStatementProviders,
            ShellSettings shellSettings,
            ISignals signals,
            IPerformanceMonitor performanceMonitor)
            : base(context, contentTypeRepository, contentItemRepository, contentItemVersionRepository, contentDefinitionManager, cacheManager, contentManagerSession, contentDisplay, sessionLocator, handlers, identityResolverSelectors, sqlStatementProviders, shellSettings, signals)
        {
            _performanceMonitor = performanceMonitor;
        }

        public new virtual ContentItem Get(int id)
        {
            return Get(id, VersionOptions.Published);
        }

        public new virtual ContentItem Get(int id, VersionOptions options)
        {
            return Get(id, options, QueryHints.Empty);
        }

        public new ContentItem Get(int id, VersionOptions options, QueryHints hints)
        {
            return _performanceMonitor.PublishTimedAction(() => base.Get(id, options, hints), r => new ContentManagerMessage
            {
                ContentId = id,
                EventCategory = TimelineCategories.ContentManagement,
                EventName = "Get: " + GetContentType(id, r, options),
                EventSubText = GetContentName(r)
            });
        }

        public new ContentItem New(string contentType)
        {
            return _performanceMonitor.PublishTimedAction(() => base.New(contentType), r => new ContentManagerMessage
            {
                ContentId = r.Id,
                EventCategory = TimelineCategories.ContentManagement,
                EventName = "New: " + r.ContentType,
                EventSubText = GetContentName(r)
            });
        }


        public new dynamic BuildDisplay(IContent content, string displayType = "", string groupId = "")
        {
            return _performanceMonitor.PublishTimedAction(() => base.BuildDisplay(content, displayType, groupId), r => new ContentManagerMessage
            {
                ContentId = content.ContentItem.Id,
                EventCategory = TimelineCategories.ContentManagement,
                EventName = "Build Display: " + content.ContentItem.ContentType,
                EventSubText = GetContentName(content)
            });
        }

        private string GetContentType(int id, ContentItem item, VersionOptions options)
        {
            if (item != null)
            {
                return item.ContentType;
            }
            return (options.VersionRecordId == 0) ? String.Format("Content item: {0} is not published.", id) : "Unknown content type.";
        }

        private string GetContentName(IContent content)
        {
            if (content.Has<TitlePart>()) { return content.As<TitlePart>().Title; }
            if (content.Has<WidgetPart>()) { return content.As<WidgetPart>().Title; }
            if (content.Has<UserPart>()) { return content.As<UserPart>().UserName; }
            if (content.Has<LayerPart>()) { return content.As<LayerPart>().Name; }

            return "Unknown";
        }
    }
}
