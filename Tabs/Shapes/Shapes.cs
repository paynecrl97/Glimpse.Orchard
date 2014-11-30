using System;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;
using Glimpse.Orchard.PerfMon.Models;

namespace Glimpse.Orchard.Tabs.Shapes
{
    public class ShapeMessage : MessageBase, ITimedPerfMonMessage
    {
        public string Name { get; set; }
        public TimeSpan Offset { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class ShapeTab : TabBase, ITabSetup, IKey, ITabLayout
    {
        private static readonly object layout = TabLayout.Create()
            .Row(r =>
            {
                r.Cell(0);
                r.Cell(1);
                r.Cell(2);
                r.Cell(3).Suffix(" ms").AlignRight().Prefix("T+ ").Class("mono");
                r.Cell(4).Suffix(" ms").AlignRight().Class("mono");
            });

        public override object GetData(ITabContext context)
        {
            return context.GetMessages<ShapeMessage>();
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

        public object GetLayout()
        {
            return layout;
        }
    }

    //public class ShapeMessagesConverter : SerializationConverter<IEnumerable<LayerMessage>>
    //{
    //    public override object Convert(IEnumerable<LayerMessage> messages)
    //    {
    //        var root = new TabSection("Layer Name", "Layer Rule", "Active", "From Request Start", "From Last");
    //        foreach (var message in messages)
    //        {
    //            root.AddRow()
    //                .Column(message.Name)
    //                .Column(message.Rule)
    //                .Column(message.Active)
    //                .Column(message.FromFirst)
    //                .Column(message.FromLast);
    //        }

    //        return root.Build();
    //    }
    //}
}