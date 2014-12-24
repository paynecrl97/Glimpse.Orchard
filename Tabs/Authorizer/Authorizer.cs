using System;
using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;
using ITimedMessage = Glimpse.Orchard.PerfMon.Models.ITimedPerfMonMessage;

namespace Glimpse.Orchard.Tabs.Authorizer
{
    public class AuthorizerMessage : MessageBase
    {
        public string PermissionName { get; set; }
        public bool UserIsAuthorized { get; set; }
        public int ContentId { get; set; }
        public string ContentName { get; set; }
        public string ContentType { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class AuthorizerManagerTab : TabBase, ITabSetup, IKey
    {

        public override object GetData(ITabContext context)
        {
            if (context.GetMessages<AuthorizerMessage>().Any())
            {
                return context.GetMessages<AuthorizerMessage>().ToList();
            }

            return "There have been no Authorization events recorded. If you think there should have been, check that the 'Glimpse for Orchard Authorizer' feature is enabled.";
        }

        public override string Name
        {
            get { return "Authorizer"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<AuthorizerMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_authorizer"; }
        }
    }

    public class AuthorizerMessagesConverter : SerializationConverter<IEnumerable<AuthorizerMessage>>
    {
        public override object Convert(IEnumerable<AuthorizerMessage> messages)
        {
            var root = new TabSection("Permission Name", "User is Authorized", "Content Id", "Content Name", "Content Type", "Evaluation Time");
            foreach (var message in messages)
            {
                root.AddRow()
                    .Column(message.PermissionName)
                    .Column(message.UserIsAuthorized ? "Yes" : "No")
                    .Column((message.ContentId == 0 ? null : message.ContentId.ToString()))
                    .Column(message.ContentName)
                    .Column(message.ContentType)
                    .Column(string.Format("{0} ms", Math.Round(message.Duration.TotalMilliseconds, 2)))
                    .QuietIf(!message.UserIsAuthorized);
            }

            return root.Build();
        }
    }
}