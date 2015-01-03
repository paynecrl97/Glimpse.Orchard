using System;
using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;
using Glimpse.Orchard.Extensions;
using Orchard.ContentManagement;

namespace Glimpse.Orchard.Tabs.Parts
{
    public class PartMessage : MessageBase
    {
        public string ContentItemName { get; set; }
        public string ContentItemType { get; set; }
        public string ContentItemStereotype { get; set; }
        public int ContentItemId { get; set; }
        public string ContentPartType { get; set; }
        public IEnumerable<ContentField> Fields { get; set; }
        public TimeSpan Duration { get; set; }
    }

    //public class ContentDefinitionVM 
    //{
    //    public Dictionary<ContentTypeSummaryVM, IEnumerable<ContentPartSummaryVM>> Content { get; set; }
    //}

    //public class ContentTypeSummaryVM
    //{
    //    public string ContentItemName { get; set; }
    //    public string ContentItemType { get; set; }
    //    public string ContentItemStereotype { get; set; }
    //    public int ContentItemId { get; set; }
    //    public IEnumerable<ContentPartSummaryVM> Parts { get; set; }
    //    public TimeSpan Duration { get; set; }

    //}

    //public class ContentPartSummaryVM
    //{
    //    public string ContentPartType { get; set; }
    //    public IEnumerable<ContentField> Fields { get; set; }
    //    public TimeSpan Duration { get; set; }
    //}

    public class PartTab : TabBase, ITabSetup, IKey
    {
        public override object GetData(ITabContext context)
        {
            var messages = context.GetMessages<PartMessage>().ToList();

            if (!messages.Any())
            {
                return "There have been no Content Part Driver events recorded. If you think there should have been, check that the 'Glimpse for Orchard Content Part Drivers' feature is enabled.";
            }

            //var vm = new ContentDefinitionVM {
            //    Content = new Dictionary<ContentTypeSummaryVM, IEnumerable<ContentPartSummaryVM>>()
            //};

            //foreach (var contentItem in messages.Select(m=> new ContentTypeSummaryVM{ContentItemId = m.ContentItemId}).Distinct().ToList()) {
            //    var scopedContentItem = contentItem;
            //    vm.Content.Add(contentItem, messages.Where(m => m.ContentItemId == scopedContentItem.ContentItemId).Select(m=> new ContentPartSummaryVM{ContentPartType = m.ContentPartType}));
            //}

            return messages;
        }

        public override string Name
        {
            get { return "Part Drivers"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<PartMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_parts"; }
        }

    }

    public class PartMessagesConverter : SerializationConverter<IEnumerable<PartMessage>>
    {
        public override object Convert(IEnumerable<PartMessage> messages)
        {
            var root = new TabSection("Content Item Name", "Content Item Type", "Content Item Stereotype", "Content Part", "Fields", "Duration");
            foreach (var message in messages.OrderByDescending(m=>m.Duration))
            {
                root.AddRow()
                    .Column(message.ContentItemName)
                    .Column(message.ContentItemType)
                    .Column(message.ContentItemStereotype)
                    .Column(message.ContentPartType)
                    .Column(message.Fields.Any() ? message.Fields as object : "None")
                    .Column(message.Duration.ToTimingString());
            }

            root.AddRow()
                .Column("")
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