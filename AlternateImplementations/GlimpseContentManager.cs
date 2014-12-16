using System;
using System.Collections.Generic;
using Autofac;
using Glimpse.Orchard.Extensions;
using Glimpse.Orchard.Models;
using Glimpse.Orchard.PerfMon.Services;
using Glimpse.Orchard.Tabs.ContentManager;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Data.Providers;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;

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
            return _performanceMonitor.PublishTimedAction(() => base.Get(id, options, hints), (r, t) => new ContentManagerGetMessage
            {
                ContentId = id,
                ContentType = GetContentType(id, r, options),
                Name = r.GetContentName(),
                Duration = t.Duration,
                //VersionOptions = options
            }, TimelineCategories.ContentManagement, r => "Get: " + GetContentType(id, r, options), r=> r.GetContentName()).ActionResult;
        }

        public new dynamic BuildDisplay(IContent content, string displayType = "", string groupId = "")
        {
            return _performanceMonitor.PublishTimedAction(() => base.BuildDisplay(content, displayType, groupId), (r, t) => new ContentManagerBuildDisplayMessage
            {
                ContentId = content.ContentItem.Id,
                ContentType = content.ContentItem.ContentType,
                Name = content.ContentItem.GetContentName(),
                Duration = t.Duration,
                DisplayType = displayType,
                GroupId = groupId
            }, TimelineCategories.ContentManagement, "Build Display: " + content.ContentItem.ContentType, content.GetContentName()).ActionResult;
        }

        private string GetContentType(int id, ContentItem item, VersionOptions options)
        {
            if (item != null)
            {
                return item.ContentType;
            }
            return (options.VersionRecordId == 0) ? String.Format("Content item: {0} is not published.", id) : "Unknown content type.";
        }
    }
}
