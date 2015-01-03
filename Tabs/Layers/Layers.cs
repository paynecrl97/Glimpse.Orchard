using System;
using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;
using Glimpse.Orchard.Extensions;
using Glimpse.Orchard.PerfMon.Models;

namespace Glimpse.Orchard.Tabs.Layers
{
    public class TimelineMessage : MessageBase, ITimelineMessage, ITimedPerfMonMessage
    {
        public TimeSpan Offset { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public string EventName { get; set; }
        public TimelineCategoryItem EventCategory { get; set; }
        public string EventSubText { get; set; }
    }

    public class LayerMessage : MessageBase
    {
        public string Name { get; set; }
        public string Rule { get; set; }
        public bool Active { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class LayerTab : TabBase, ITabSetup, IKey
    {

        public override object GetData(ITabContext context) 
        {
            var messages = context.GetMessages<LayerMessage>().ToList();

            if (!messages.Any()) {
                return "There have been no Layer events recorded. If you think there should have been, check that the 'Glimpse for Orchard Widgets' feature is enabled.";
            }

            return messages;
        }

        public override string Name
        {
            get { return "Layers"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<LayerMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_layers"; }
        }
    }

    public class LayerMessagesConverter : SerializationConverter<IEnumerable<LayerMessage>>
    {
        public override object Convert(IEnumerable<LayerMessage> messages)
        {
            var root = new TabSection("Layer Name", "Layer Rule", "Active", "Evaluation Time");
            foreach (var message in messages.OrderByDescending(m=>m.Duration))
            {
                root.AddRow()
                    .Column(message.Name)
                    .Column(message.Rule)
                    .Column(message.Active ? "Yes" : "No")
                    .Column(message.Duration.ToTimingString())
                    .QuietIf(!message.Active);
            }

            return root.Build();
        }
    }
}