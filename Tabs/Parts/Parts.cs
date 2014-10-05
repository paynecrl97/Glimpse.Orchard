using System;
using System.Collections.Generic;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;
using Glimpse.Orchard.PerfMon.Models;

namespace Glimpse.Orchard.Tabs.Parts
{
    public class PartMessage : MessageBase, ITimelineMessage, ITimedPerfMonMessage
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Zone { get; set; }
        public TimeSpan Offset { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public string EventName { get; set; }
        public TimelineCategoryItem EventCategory { get; set; }
        public string EventSubText { get; set; }
    }

    public class PartTab : TabBase, ITabSetup, IKey, ITabLayout
    {
        private static readonly object layout = TabLayout.Create()
            .Row(r =>
            {
                r.Cell(0);
            });

        public override object GetData(ITabContext context)
        {
            return context.GetMessages<PartMessage>();
        }

        public override string Name
        {
            get { return "Parts"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<PartMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_parts"; }
        }

        public object GetLayout()
        {
            return layout;
        }
    }

    public class PartMessagesConverter : SerializationConverter<IEnumerable<PartMessage>>
    {
        public override object Convert(IEnumerable<PartMessage> messages)
        {
            var root = new TabSection("Part Name");
            foreach (var message in messages)
            {
                root.AddRow()
                    .Column(message.Name);
            }

            return root.Build();
        }
    }
}