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

namespace Glimpse.Orchard.AlternateImplementations {
    [OrchardSuppressDependency("Orchard.ContentManagement.DefaultContentManager")]
    public class GlimpseDefaultContentManager : DefaultContentManager, IContentManager
    {
        private readonly IPerformanceMonitor _performanceMonitor;

        public GlimpseDefaultContentManager(
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
            : base(context, contentTypeRepository, contentItemRepository, contentItemVersionRepository, contentDefinitionManager, cacheManager, contentManagerSession, contentDisplay, sessionLocator, handlers, identityResolverSelectors, sqlStatementProviders, shellSettings, signals) {
            _performanceMonitor = performanceMonitor;
        }

        public new virtual ContentItem Get(int id) {
            return Get(id, VersionOptions.Published);
        }

        public new virtual ContentItem Get(int id, VersionOptions options) {
            return Get(id, options, QueryHints.Empty);
        }

        public new virtual ContentItem Get(int id, VersionOptions options, QueryHints hints) {
            return _performanceMonitor.PublishTimedAction(() => base.Get(id, options, hints), r => new ContentManagerMessage {
                ContentId = id,
                EventCategory = TimelineCategories.ContentManagement,
                EventName = "Get: " + r.ContentType,
                EventSubText = GetContentName(r)
            });
        }

        //private ContentItemVersionRecord GetVersionRecord(VersionOptions options, ContentItemRecord itemRecord) {
        //    if (options.IsPublished) {
        //        return itemRecord.Versions.FirstOrDefault(
        //            x => x.Published) ??
        //               _contentItemVersionRepository.Get(
        //                   x => x.ContentItemRecord == itemRecord && x.Published);
        //    }
        //    if (options.IsLatest || options.IsDraftRequired) {
        //        return itemRecord.Versions.FirstOrDefault(
        //            x => x.Latest) ??
        //               _contentItemVersionRepository.Get(
        //                   x => x.ContentItemRecord == itemRecord && x.Latest);
        //    }
        //    if (options.IsDraft) {
        //        return itemRecord.Versions.FirstOrDefault(
        //            x => x.Latest && !x.Published) ??
        //               _contentItemVersionRepository.Get(
        //                   x => x.ContentItemRecord == itemRecord && x.Latest && !x.Published);
        //    }
        //    if (options.VersionNumber != 0) {
        //        return itemRecord.Versions.FirstOrDefault(
        //            x => x.Number == options.VersionNumber) ??
        //               _contentItemVersionRepository.Get(
        //                   x => x.ContentItemRecord == itemRecord && x.Number == options.VersionNumber);
        //    }
        //    return null;
        //}

        //public new IEnumerable<T> GetMany<T>(IEnumerable<int> ids, VersionOptions options, QueryHints hints) where T : class, IContent {
        //    var contentItemVersionRecords = GetManyImplementation(hints, (contentItemCriteria, contentItemVersionCriteria) => {
        //        contentItemCriteria.Add(Restrictions.In("Id", ids.ToArray()));
        //        if (options.IsPublished) {
        //            contentItemVersionCriteria.Add(Restrictions.Eq("Published", true));
        //        }
        //        else if (options.IsLatest) {
        //            contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
        //        }
        //        else if (options.IsDraft && !options.IsDraftRequired) {
        //            contentItemVersionCriteria.Add(
        //                Restrictions.And(Restrictions.Eq("Published", false),
        //                                Restrictions.Eq("Latest", true)));
        //        }
        //        else if (options.IsDraft || options.IsDraftRequired) {
        //            contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
        //        }
        //    });

        //    var itemsById = contentItemVersionRecords
        //        .Select(r => Get(r.ContentItemRecord.Id, options.IsDraftRequired ? options : VersionOptions.VersionRecord(r.Id)))
        //        .GroupBy(ci => ci.Id)
        //        .ToDictionary(g => g.Key);

        //    return ids.SelectMany(id => {
        //            IGrouping<int, ContentItem> values;
        //            return itemsById.TryGetValue(id, out values) ? values : Enumerable.Empty<ContentItem>();
        //        }).AsPart<T>().ToArray();
        //}

        //public new GroupInfo GetDisplayGroupInfo(IContent content, string groupInfoId) {
        //    return GetDisplayGroupInfos(content).FirstOrDefault(gi => string.Equals(gi.Id, groupInfoId, StringComparison.OrdinalIgnoreCase));
        //}

        public new dynamic BuildDisplay(IContent content, string displayType = "", string groupId = "") {
            return _performanceMonitor.PublishTimedAction(() => base.BuildDisplay(content, displayType, groupId), r => new ContentManagerMessage {
                ContentId = content.ContentItem.Id,
                EventCategory = TimelineCategories.ContentManagement,
                EventName = "Build Display: " + content.ContentItem.ContentType,
                EventSubText = GetContentName(content)
            });
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
