using System;
using System.Collections.Generic;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;

namespace Glimpse.Orchard.Tabs.Fields
{
    public class FieldMessage : MessageBase, ITimelineMessage
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

    public class FieldTab : TabBase, ITabSetup, IKey, ITabLayout
    {
        private static readonly object layout = TabLayout.Create()
            .Row(r =>
            {
                r.Cell(0);
            });

        public override object GetData(ITabContext context)
        {
            return context.GetMessages<FieldMessage>();
        }

        public override string Name
        {
            get { return "Fields"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<FieldMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_fields"; }
        }

        public object GetLayout()
        {
            return layout;
        }
    }

    public class FieldMessagesConverter : SerializationConverter<IEnumerable<FieldMessage>>
    {
        public override object Convert(IEnumerable<FieldMessage> messages)
        {
            var root = new TabSection("Field Name");
            foreach (var message in messages)
            {
                root.AddRow()
                    .Column(message.Name);
            }

            return root.Build();
        }
    }
}