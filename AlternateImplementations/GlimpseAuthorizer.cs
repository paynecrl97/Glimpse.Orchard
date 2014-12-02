using Glimpse.Orchard.Extensions;
using Glimpse.Orchard.Models;
using Glimpse.Orchard.PerfMon.Services;
using Glimpse.Orchard.Tabs.Authorizer;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.UI.Notify;

namespace Glimpse.Orchard.AlternateImplementations {
    [OrchardSuppressDependency("Orchard.Security.Authorizer")]
    public class GlimpseAuthorizer : Authorizer, IAuthorizer {
        private readonly IPerformanceMonitor _performanceMonitor;

        public GlimpseAuthorizer(
            IAuthorizationService authorizationService,
            INotifier notifier,
            IWorkContextAccessor workContextAccessor,
            IPerformanceMonitor performanceMonitor)
            : base(authorizationService, notifier, workContextAccessor) {
            _performanceMonitor = performanceMonitor;
        }

        public new bool Authorize(Permission permission) {
            return Authorize(permission, null, null);
        }

        public new bool Authorize(Permission permission, LocalizedString message) {
            return Authorize(permission, null, message);
        }

        public new bool Authorize(Permission permission, IContent content) {
            return Authorize(permission, content, null);
        }

        public new bool Authorize(Permission permission, IContent content, LocalizedString message) {
            var authorizerMessage = new AuthorizerMessage {PermissionName = permission.Name};

            if (content != null)
            {
                authorizerMessage.ContentId = content.Id;
                authorizerMessage.ContentType = content.ContentItem.ContentType;
                authorizerMessage.ContentName = content.GetContentName();
            }

            return _performanceMonitor.PublishTimedAction(() => base.Authorize(permission, content, message), (r, t) => { 
                authorizerMessage.Duration = t.Duration;
                authorizerMessage.UserIsAuthorized = r;

                return authorizerMessage;
            }, TimelineCategories.Authorization, "Authorize", permission.Name).ActionResult;
        }

    }
}
