using System;
using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;

namespace Glimpse.Orchard.Tabs.Layers
{
    public class LayerMessage : MessageBase, ITimelineMessage
    {
        public string Name { get; set; }
        public string Rule { get; set; }
        public bool Active { get; set; }
        public TimeSpan Offset { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public string EventName { get; set; }
        public TimelineCategoryItem EventCategory { get; set; }
        public string EventSubText { get; set; }
    }

    public class LayerTab : TabBase, ITabSetup, IKey, ITabLayout
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
            return context.GetMessages<LayerMessage>().ToList();
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

        public object GetLayout()
        {
            return layout;
        }
    }

    public class LayerMessagesConverter : SerializationConverter<IEnumerable<LayerMessage>>
    {
        public override object Convert(IEnumerable<LayerMessage> messages)
        {
            var root = new TabSection("Layer Name", "Layer Rule", "Active", "Evaluation Time");
            foreach (var message in messages)
            {
                root.AddRow()
                    .Column(message.Name)
                    .Column(message.Rule)
                    .Column(message.Active)
                    .Column(message.Duration)
                    .StrongIf(message.Active);
            }

            return root.Build();
        }
    }
}