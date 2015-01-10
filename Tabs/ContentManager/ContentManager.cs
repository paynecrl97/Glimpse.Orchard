using System;
using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;
using Glimpse.Orchard.Extensions;
using Orchard.ContentManagement;

namespace Glimpse.Orchard.Tabs.ContentManager
{
    public class ContentManagerMessage : MessageBase
    {
        public int ContentId { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public VersionOptions VersionOptions { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class ContentManagerTab : TabBase, ITabSetup, IKey, ILayoutControl
    {
        public override object GetData(ITabContext context) {
            var messages = context.GetMessages<ContentManagerMessage>().ToList();

            if (!messages.Any())
            {
                return "There have been no Display Manager events recorded. If you think there should have been, check that the 'Glimpse for Orchard Content Manager' feature is enabled.";
            }

            return messages;
        }

        public override string Name
        {
            get { return "Content Manager"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<ContentManagerMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_contentmanager"; }
        }

        public bool KeysHeadings { get { return true; } }
    }

    public class ContentManagerGetMessagesConverter : SerializationConverter<IEnumerable<ContentManagerMessage>>
    {
        public override object Convert(IEnumerable<ContentManagerMessage> messages)
        {
            var root = new TabSection("Content Id", "Content Type", "Name", "Version Options", "Duration");
            foreach (var message in messages.OrderByDescending(m => m.Duration))
            {
                root.AddRow()
                    .Column(message.ContentId)
                    .Column(message.ContentType)
                    .Column(message.Name)
                    .Column(message.VersionOptions)
                    .Column(message.Duration.ToTimingString());
            }

            root.AddRow()
                .Column("")
                .Column("")
                .Column("")
                .Column("Total time:")
                .Column(messages.Sum(m => m.Duration.TotalMilliseconds).ToTimingString())
                .Selected();

            return root.Build();
        }
    }
}