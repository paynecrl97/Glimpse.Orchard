using System;
using System.Collections.Generic;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Glimpse.Core.Tab.Assist;

namespace Glimpse.Orchard.Tabs.Cache
{
    public class CacheMessage : MessageBase
    {
        public string Action { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }
        public string Result { get; set; }
        public TimeSpan ValidFor { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class CacheTab : TabBase, ITabSetup, IKey
    {

        public override object GetData(ITabContext context)
        {
            return context.GetMessages<CacheMessage>();
        }

        public override string Name
        {
            get { return "Cache"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<CacheMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_cache"; }
        }
    }

    public class CacheMessagesConverter : SerializationConverter<IEnumerable<CacheMessage>>
    {
        public override object Convert(IEnumerable<CacheMessage> messages)
        {
            var root = new TabSection("Action", "Key", "Result", "Value", "Time Taken");
            foreach (var message in messages)
            {
                root.AddRow()
                    .Column(message.Action)
                    .Column(message.Key)
                    .Column(message.Result)
                    .Column(message.Value)
                    .Column(string.Format("{0} ms", Math.Round(message.Duration.TotalMilliseconds, 2)));
            }

            return root.Build();
        }
    }
}