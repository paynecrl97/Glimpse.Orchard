using System;
using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;
using Glimpse.Orchard.PerfMon.Models;

namespace Glimpse.Orchard.Tabs.ContentManager
{
    public class ContentManagerMessage : MessageBase, ITimelineMessage, ITimedPerfMonMessage
    {
        public int ContentId { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public TimeSpan Offset { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public string EventName { get; set; }
        public TimelineCategoryItem EventCategory { get; set; }
        public string EventSubText { get; set; }
    }

    public class ContentManagerTab : TabBase, ITabSetup, IKey, ITabLayout
    {
        private static readonly object layout = TabLayout.Create()
            .Row(r =>
            {
                r.Cell(0);
                r.Cell(1);
                r.Cell(2);
                r.Cell(3).Suffix(" ms").AlignRight().Class("mono");
            });

        public override object GetData(ITabContext context)
        {
            return context.GetMessages<ContentManagerMessage>().ToList();
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

        public object GetLayout()
        {
            return layout;
        }
    }

    public class ContentManagerMessagesConverter : SerializationConverter<IEnumerable<ContentManagerMessage>>
    {
        public override object Convert(IEnumerable<ContentManagerMessage> messages)
        {
            var root = new TabSection("Content Id", "Name", "Content Type", "Evaluation Time");
            foreach (var message in messages)
            {
                root.AddRow()
                    .Column(message.ContentId)
                    .Column(message.Name)
                    .Column(message.ContentType)
                    .Column(message.Duration);
            }

            return root.Build();
        }
    }
}