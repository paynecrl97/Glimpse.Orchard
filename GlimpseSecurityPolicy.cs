//using System.Web;
//using System.Web.Mvc;
//using Autofac;
//using Autofac.Integration.Web;
//using Glimpse.AspNet.Extensions;
//using Glimpse.Core.Extensibility;
//using Orchard;
//using Orchard.Security;

//namespace Glimpse.Orchard
//{
//    public class GlimpseSecurityPolicy:IRuntimePolicy
//    {
//        public GlimpseSecurityPolicy() {
//            var x = DependencyResolver.Current.GetService<IOrchardServices>();

//            var cpa = (IContainerProviderAccessor)HttpContext.Current.ApplicationInstance;
//            var y = cpa.ContainerProvider.RequestLifetime.Resolve<IOrchardServices>();
//        }

//        public RuntimePolicy Execute(IRuntimePolicyContext policyContext)
//        {
//             //You can perform a check like the one below to control Glimpse's permissions within your application.
//             //More information about RuntimePolicies can be found at http://getglimpse.com/Help/Custom-Runtime-Policy
//             var httpContext = policyContext.GetHttpContext();
//             if (!httpContext.User.IsInRole("Administrator"))
//             {
//                 return RuntimePolicy.Off;
//             }

//            return RuntimePolicy.On;
//        }

//        public RuntimeEvent ExecuteOn
//        {
//             //The RuntimeEvent.ExecuteResource is only needed in case you create a security policy
//             //Have a look at http://blog.getglimpse.com/2013/12/09/protect-glimpse-axd-with-your-custom-runtime-policy/ for more details
//            get { return RuntimeEvent.EndRequest | RuntimeEvent.ExecuteResource; }
//        }
//    }
//}