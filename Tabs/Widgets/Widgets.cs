using System;
using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;
using Orchard.Widgets.Models;

namespace Glimpse.Orchard.Tabs.Widgets
{
    public class WidgetMessage : MessageBase
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Zone { get; set; }
        public string Position { get; set; }
        public string TechnicalName { get; set; }
        public LayerPart Layer { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class WidgetTab : TabBase, ITabSetup, IKey
    {

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
    }

    public class WidgetMessagesConverter : SerializationConverter<IEnumerable<WidgetMessage>>
    {
        public override object Convert(IEnumerable<WidgetMessage> messages)
        {
            var root = new TabSection("Widget Title", "Widget Type", "Layer", "Layer Rule", "Zone", "Position", "Technical Name", "Build Display Duration");
            foreach (var message in messages.OrderByDescending(m=>m.Duration.TotalMilliseconds))
            {
                root.AddRow()
                    .Column(message.Title)
                    .Column(message.Type)
                    .Column(message.Layer.Name)
                    .Column(message.Layer.LayerRule)
                    .Column(message.Zone)
                    .Column(message.Position)
                    .Column(message.TechnicalName)
                    .Column(string.Format("{0} ms", Math.Round(message.Duration.TotalMilliseconds, 2)));
            }

            return root.Build();
        }
    }
}