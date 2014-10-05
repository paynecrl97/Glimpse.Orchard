using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Glimpse.Orchard.Permissions {
    public class Permissions : IPermissionProvider {
        public static readonly Permission GlimpseViewer = new Permission { Description = "Users with this permission can use the Glimpse performance profiling tool", Name = "GlimpseViewer" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                GlimpseViewer,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {GlimpseViewer}
                },
            };
        }
    }
}
