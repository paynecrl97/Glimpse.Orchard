using System;
using System.Collections.Generic;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;

namespace Glimpse.Orchard.Tabs.Widgets
{
    public class WidgetMessage : MessageBase, ITimelineMessage
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

    public class WidgetTab : TabBase, ITabSetup, IKey, ITabLayout
    {
        private static readonly object layout = TabLayout.Create()
            .Row(r =>
            {
                r.Cell(0);
            });

        public override object GetData(ITabContext context)
        {
            return context.GetMessages<WidgetMessage>();
        }

        public override string Name
        {
            get { return "Widgets"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<WidgetMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_widgets"; }
        }

        public object GetLayout()
        {
            return layout;
        }
    }

    public class WidgetMessagesConverter : SerializationConverter<IEnumerable<WidgetMessage>>
    {
        public override object Convert(IEnumerable<WidgetMessage> messages)
        {
            var root = new TabSection("Widget Name");
            foreach (var message in messages)
            {
                root.AddRow()
                    .Column(message.Name);
            }

            return root.Build();
        }
    }
}