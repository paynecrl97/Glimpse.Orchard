using System;
using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;
using Glimpse.Orchard.Extensions;
using Orchard.DisplayManagement.Shapes;

namespace Glimpse.Orchard.Tabs.Shapes
{
    public class ShapeMessage : MessageBase {
        private readonly ShapeMetadata _metaData;
        public ShapeMessage(ShapeMetadata metaData) {
            _metaData = metaData;
        }

        public TimeSpan Duration { get; set; }
        public string BindingName { get; set; }
        public string BindingSource { get; set; }
        public string Type { get { return _metaData.Type; } }
        public string DisplayType { get { return _metaData.DisplayType; } }
        public string Position { get { return _metaData.Position; } }
        public string PlacementSource { get { return _metaData.PlacementSource; } }
        public string Prefix { get { return _metaData.Prefix; } }
        public IList<string> Wrappers { get { return _metaData.Wrappers.Any() ? _metaData.Wrappers : null; } }
        public IList<string> Alternates { get { return _metaData.Alternates.Any() ? _metaData.Alternates : null; } }
        public IList<string> BindingSources { get { return _metaData.BindingSources.Any() ? _metaData.BindingSources : null; } }
    }

    public class ShapeTab : TabBase, ITabSetup, IKey
    {
        public override object GetData(ITabContext context)
        {
            var messages = context.GetMessages<ShapeMessage>().ToList();

            if (!messages.Any())
            {
                return "There have been no Shape events recorded. If you think there should have been, check that the 'Glimpse for Orchard Display Manager' feature is enabled.";
            }

            return messages;
        }

        public override string Name
        {
            get { return "Shapes"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<ShapeMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_shapes"; }
        }
    }

    public class ShapeMessagesConverter : SerializationConverter<IEnumerable<ShapeMessage>>
    {
        public override object Convert(IEnumerable<ShapeMessage> messages)
        {
            var root = new TabSection("Type", "DisplayType", "Position", "Placement Source", "Prefix", "Binding Source", "Available Binding Sources", "Wrappers", "Alternates", "Build Display Duration");
            foreach (var message in messages.OrderByDescending(m=>m.Duration.TotalMilliseconds)) {
                if (message.Type != "Layout" //these exemptions are taken from the Shape Tracing Feature
                    && message.Type != "DocumentZone"
                    && message.Type != "PlaceChildContent"
                    && message.Type != "ContentZone"
                    && message.Type != "ShapeTracingMeta"
                    && message.Type != "ShapeTracingTemplates"
                    && message.Type != "DateTimeRelative")
                {
                    root.AddRow()
                        .Column(message.Type)
                        .Column(message.DisplayType)
                        .Column(message.Position)
                        .Column(message.PlacementSource)
                        .Column(message.Prefix)
                        .Column(message.BindingSource)
                        .Column(message.BindingSources)
                        .Column(message.Wrappers)
                        .Column(message.Alternates)
                        .Column(message.Duration.ToTimingString());
                }
            }

            return root.Build();
        }
    }
}