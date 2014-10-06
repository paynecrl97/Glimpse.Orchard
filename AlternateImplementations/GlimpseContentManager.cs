using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Glimpse.Orchard.Models;
using Glimpse.Orchard.PerfMon.Services;
using Glimpse.Orchard.Tabs.ContentManager;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Orchard;
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
    public class GlimpseContentManager : DefaultContentManager, IContentManager
    {
        private readonly IPerformanceMonitor _performanceMonitor;
        private readonly Func<IContentManagerSession> _contentManagerSession;
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;
        private readonly IRepository<ContentItemRecord> _contentItemRepository;
        private readonly Lazy<ISessionLocator> _sessionLocator;

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
            : base(context, contentTypeRepository, contentItemRepository, contentItemVersionRepository, contentDefinitionManager, cacheManager, contentManagerSession, contentDisplay, sessionLocator, handlers, identityResolverSelectors, sqlStatementProviders, shellSettings, signals) {
            _performanceMonitor = performanceMonitor;

            _contentManagerSession = contentManagerSession;
            _contentItemVersionRepository = contentItemVersionRepository;
            _contentItemRepository = contentItemRepository;
            _sessionLocator = sessionLocator;
        }

        public new virtual ContentItem Get(int id) {
            return Get(id, VersionOptions.Published);
        }

        public new virtual ContentItem Get(int id, VersionOptions options) {
            return Get(id, options, QueryHints.Empty);
        }

        //public new virtual ContentItem Get(int id, VersionOptions options, QueryHints hints)
        //{
        //    return _performanceMonitor.PublishTimedAction(() => base.Get(id, options, hints), r => new ContentManagerMessage
        //    {
        //        ContentId = id,
        //        EventCategory = TimelineCategories.ContentManagement,
        //        EventName = "Get: " + r.ContentType,
        //        EventSubText = GetContentName(r)
        //    });
        //}

        public virtual ContentItem Get(int id, VersionOptions options, QueryHints hints)
        {
            return _performanceMonitor.PublishTimedAction(() => {
                Trace.WriteLine("Entered Get- ID:" + id);

                var session = _contentManagerSession();
                ContentItem contentItem;

                ContentItemVersionRecord versionRecord = null;

                // obtain the root records based on version options
                if (options.VersionRecordId != 0)
                {
                    Trace.WriteLine("options.VersionRecordId != 0");
                    // short-circuit if item held in session
                    if (session.RecallVersionRecordId(options.VersionRecordId, out contentItem)) {
                        Trace.WriteLine("Content item was recalled from the session");
                        return contentItem;
                    }

                    Trace.WriteLine("Content item was not in the session");
                    versionRecord = _contentItemVersionRepository.Get(options.VersionRecordId);
                    Trace.WriteLine("Got versionRecord from repo");
                }
                else if (session.RecallContentRecordId(id, out contentItem)) {
                    // try to reload a previously loaded published content item

                    Trace.WriteLine("options.VersionRecordId == 0 and Content item was recalled from the session");
                    if (options.IsPublished)
                    {
                        Trace.WriteLine("returning published content item");
                        return contentItem;
                    }

                    versionRecord = contentItem.VersionRecord;
                }
                else
                {
                    Trace.WriteLine("Calling GetManyImplementation to get contentitemversionrecords");
                    // do a query to load the records in case Get is called directly
                    var contentItemVersionRecords = GetManyImplementation(hints,
                        (contentItemCriteria, contentItemVersionCriteria) => {
                            contentItemCriteria.Add(Restrictions.Eq("Id", id));
                            if (options.IsPublished) {
                                contentItemVersionCriteria.Add(Restrictions.Eq("Published", true));
                            }
                            else if (options.IsLatest) {
                                contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
                            }
                            else if (options.IsDraft && !options.IsDraftRequired) {
                                contentItemVersionCriteria.Add(
                                    Restrictions.And(Restrictions.Eq("Published", false),
                                        Restrictions.Eq("Latest", true)));
                            }
                            else if (options.IsDraft || options.IsDraftRequired) {
                                contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
                            }

                            contentItemVersionCriteria.SetFetchMode("ContentItemRecord", FetchMode.Eager);
                            contentItemVersionCriteria.SetFetchMode("ContentItemRecord.ContentType", FetchMode.Eager);
                            contentItemVersionCriteria.SetMaxResults(1);
                        });
                    Trace.WriteLine("finished");

                    if (options.VersionNumber != 0)
                    {
                        Trace.WriteLine("options.versionnumber != 0");
                        versionRecord = contentItemVersionRecords.FirstOrDefault(
                            x => x.Number == options.VersionNumber) ??
                                        _contentItemVersionRepository.Get(
                                            x => x.ContentItemRecord.Id == id && x.Number == options.VersionNumber);
                        Trace.WriteLine("got version record");
                    }
                    else
                    {
                        Trace.WriteLine("options.versionnumber == 0");
                        versionRecord = contentItemVersionRecords.FirstOrDefault();
                        Trace.WriteLine("got version record from the result of GetManyImplementation");
                    }
                }


                Trace.WriteLineIf(versionRecord==null, "version record is null");

                // no record means content item is not in db
                if (versionRecord == null) {
                    // check in memory
                    var record = _contentItemRepository.Get(id);
                    if (record == null) {
                        return null;
                    }

                    versionRecord = GetVersionRecord(options, record);

                    if (versionRecord == null) {
                        return null;
                    }
                }

                Trace.WriteLine("getting version from session");
                // return item if obtained earlier in session
                if (session.RecallVersionRecordId(versionRecord.Id, out contentItem))
                {
                    Trace.WriteLine("got content item from session");
                    if (options.IsDraftRequired && versionRecord.Published)
                    {
                        Trace.WriteLine("draft version is required- building new version");
                        return BuildNewVersion(contentItem);
                    }
                    return contentItem;
                }

                // allocate instance and set record property
                Trace.WriteLine("creating new content item");
                contentItem = New(versionRecord.ContentItemRecord.ContentType.Name);
                Trace.WriteLine("new content item created");
                contentItem.VersionRecord = versionRecord;

                // store in session prior to loading to avoid some problems with simple circular dependencies
                Trace.WriteLine("storing in session");
                session.Store(contentItem);
                Trace.WriteLine("content item placed into session");

                // create a context with a new instance to load            
                var context = new LoadContentContext(contentItem);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks

                Trace.WriteLine("invoking handler loading");
                Handlers.Invoke(handler => handler.Loading(context), Logger);
                Trace.WriteLine("invoking handlers loaded");
                Handlers.Invoke(handler => handler.Loaded(context), Logger);
                Trace.WriteLine("handlers finished");

                // when draft is required and latest is published a new version is appended 
                if (options.IsDraftRequired && versionRecord.Published) {
                    contentItem = BuildNewVersion(context.ContentItem);
                }

                return contentItem;
            }, r => new ContentManagerMessage
            {
                ContentId = id,
                EventCategory = TimelineCategories.ContentManagement,
                EventName = "Get: " + r.ContentType,
                EventSubText = GetContentName(r)
            });
        }

        private IEnumerable<ContentItemVersionRecord> GetManyImplementation(QueryHints hints, Action<ICriteria, ICriteria> predicate)
        {
            var session = _sessionLocator.Value.For(typeof(ContentItemRecord));
            var contentItemVersionCriteria = session.CreateCriteria(typeof(ContentItemVersionRecord));
            var contentItemCriteria = contentItemVersionCriteria.CreateCriteria("ContentItemRecord");
            predicate(contentItemCriteria, contentItemVersionCriteria);

            var contentItemMetadata = session.SessionFactory.GetClassMetadata(typeof(ContentItemRecord));
            var contentItemVersionMetadata = session.SessionFactory.GetClassMetadata(typeof(ContentItemVersionRecord));

            if (hints != QueryHints.Empty)
            {
                // break apart and group hints by their first segment
                var hintDictionary = hints.Records
                    .Select(hint => new { Hint = hint, Segments = hint.Split('.') })
                    .GroupBy(item => item.Segments.FirstOrDefault())
                    .ToDictionary(grouping => grouping.Key, StringComparer.InvariantCultureIgnoreCase);

                // locate hints that match properties in the ContentItemVersionRecord
                foreach (var hit in contentItemVersionMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key]))
                {
                    contentItemVersionCriteria.SetFetchMode(hit.Hint, FetchMode.Eager);
                    hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemVersionCriteria, ExtendCriteria);
                }

                // locate hints that match properties in the ContentItemRecord
                foreach (var hit in contentItemMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key]))
                {
                    contentItemVersionCriteria.SetFetchMode("ContentItemRecord." + hit.Hint, FetchMode.Eager);
                    hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemCriteria, ExtendCriteria);
                }

                if (hintDictionary.SelectMany(x => x.Value).Any(x => x.Segments.Count() > 1))
                    contentItemVersionCriteria.SetResultTransformer(new DistinctRootEntityResultTransformer());
            }

            contentItemCriteria.SetCacheable(true);

            return contentItemVersionCriteria.List<ContentItemVersionRecord>();
        }

        private static ICriteria ExtendCriteria(ICriteria criteria, string segment)
        {
            return criteria.GetCriteriaByPath(segment) ?? criteria.CreateCriteria(segment, JoinType.LeftOuterJoin);
        }

        private ContentItemVersionRecord GetVersionRecord(VersionOptions options, ContentItemRecord itemRecord)
        {
            if (options.IsPublished)
            {
                return itemRecord.Versions.FirstOrDefault(
                    x => x.Published) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Published);
            }
            if (options.IsLatest || options.IsDraftRequired)
            {
                return itemRecord.Versions.FirstOrDefault(
                    x => x.Latest) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Latest);
            }
            if (options.IsDraft)
            {
                return itemRecord.Versions.FirstOrDefault(
                    x => x.Latest && !x.Published) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Latest && !x.Published);
            }
            if (options.VersionNumber != 0)
            {
                return itemRecord.Versions.FirstOrDefault(
                    x => x.Number == options.VersionNumber) ??
                       _contentItemVersionRepository.Get(
                           x => x.ContentItemRecord == itemRecord && x.Number == options.VersionNumber);
            }
            return null;
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
