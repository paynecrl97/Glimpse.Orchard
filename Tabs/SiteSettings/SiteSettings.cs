using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;

namespace Glimpse.Orchard.Tabs.SiteSettings
{
    public class SiteSettingsMessage : MessageBase
    {
        public string Part { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class EnabledFeaturesTab : TabBase, ITabSetup, IKey
    {
        public override object GetData(ITabContext context)
        {
            if (context.GetMessages<SiteSettingsMessage>().Any())
            {
                return context.GetMessages<SiteSettingsMessage>().ToList();
            }

            return "There is no data available for this tab, check that the 'Glimpse for Orchard Site Settings' feature is enabled.";
        }

        public override string Name
        {
            get { return "Site Settings"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<SiteSettingsMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_sitesettings"; }
        }
    }

    public class EnabledFeatureMessagesConverter : SerializationConverter<IEnumerable<SiteSettingsMessage>>
    {
        public override object Convert(IEnumerable<SiteSettingsMessage> messages)
        {
            var root = new TabSection("Part", "Name", "Value");
            foreach (var message in messages.OrderBy(m => m.Part).ThenBy(m => m.Name))
            {
                root.AddRow()
                    .Column(message.Part)
                    .Column(message.Name)
                    .Column(message.Value);
            }

            return root.Build();
        }
    }
}