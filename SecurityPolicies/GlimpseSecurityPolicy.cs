//using System.Web;
//using Glimpse.AspNet.Extensions;
//using Glimpse.Core.Extensibility;
//using Orchard;
//using Orchard.Security;

//namespace Glimpse.Orchard.SecurityPolicies
//{
//    public class GlimpseSecurityPolicy : IRuntimePolicy
//    {
//        public RuntimePolicy Execute(IRuntimePolicyContext policyContext)
//        {
//            //var context = policyContext.GetHttpContext();
//            //var workContext = context.Request.RequestContext.GetWorkContext();

//            var context = HttpContext.Current;
//            var workContext = context.Request.RequestContext.GetWorkContext();

//            var authorizer = workContext.Resolve<IAuthorizer>();

//            if (authorizer.Authorize(Permissions.Permissions.GlimpseViewer))
//            {
//                return RuntimePolicy.On;
//            }

//            return RuntimePolicy.Off;
//        }

//        public RuntimeEvent ExecuteOn
//        {
//            // The RuntimeEvent.ExecuteResource is only needed in case you create a security policy
//            // Have a look at http://blog.getglimpse.com/2013/12/09/protect-glimpse-axd-with-your-custom-runtime-policy/ for more details
//            get { return RuntimeEvent.EndRequest | RuntimeEvent.ExecuteResource; }
//        }
//    }
//}
