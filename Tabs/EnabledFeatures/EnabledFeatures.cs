using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;
using Orchard.Environment.Extensions.Models;

namespace Glimpse.Orchard.Tabs.EnabledFeatures
{
    public class EnabledFeatureMessage : MessageBase
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string FeatureId { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public ExtensionDescriptor Extension { get; set; }
    }

    public class EnabledFeaturesTab : TabBase, ITabSetup, IKey
    {
        public override object GetData(ITabContext context)
        {
            if (context.GetMessages<EnabledFeatureMessage>().Any())
            {
                return context.GetMessages<EnabledFeatureMessage>().ToList();
            }

            return "There is no data available for this tab, check that the 'Glimpse for Orchard Enabled Features' feature is enabled.";
        }

        public override string Name
        {
            get { return "Enabled Features"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<EnabledFeatureMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_enabledfeatures"; }
        }
    }

    public class EnabledFeatureMessagesConverter : SerializationConverter<IEnumerable<EnabledFeatureMessage>>
    {
        public override object Convert(IEnumerable<EnabledFeatureMessage> messages)
        {
            var root = new TabSection("Feature Type", "Category", "Name", "Description", "Id", "Priority");
            foreach (var message in messages.OrderBy(m => m.Extension.ExtensionType).ThenBy(m => m.Category).ThenBy(m => m.Name))
            {
                root.AddRow()
                    .Column(message.Extension.ExtensionType)
                    .Column(message.Category)
                    .Column(message.Name)
                    .Column(message.Description)
                    .Column(message.FeatureId)
                    .Column(message.Priority);
            }

            return root.Build();
        }
    }
}