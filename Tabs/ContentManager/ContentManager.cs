using System;
using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Message;
using Orchard.ContentManagement;

namespace Glimpse.Orchard.Tabs.ContentManager
{
    public class ContentManagerMessage : MessageBase
    {
        public int ContentId { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class ContentManagerGetMessage : ContentManagerMessage
    { 
        public VersionOptions VersionOptions { get; set; }
    }

    public class ContentManagerBuildDisplayMessage : ContentManagerMessage
    {
        public string DisplayType { get; set; }
        public string GroupId { get; set; }
    }

    public class ContentManagerVM
    {
        public ContentManagerVM()
        {
            GetEvents = new List<ContentManagerGetMessage>();
            BuildDisplayEvents = new List<ContentManagerBuildDisplayMessage>();
        }

        public List<ContentManagerGetMessage> GetEvents { get; private set; }
        public List<ContentManagerBuildDisplayMessage> BuildDisplayEvents { get; private set; } 
    }

    public class ContentManagerTab : TabBase, ITabSetup, IKey, ILayoutControl
    {
        public override object GetData(ITabContext context) {
            var vm = new ContentManagerVM();
            vm.GetEvents.AddRange(context.GetMessages<ContentManagerGetMessage>().ToList());
            vm.BuildDisplayEvents.AddRange(context.GetMessages<ContentManagerBuildDisplayMessage>());

            if (!vm.GetEvents.Any() && ! vm.BuildDisplayEvents.Any())
            {
                return "There have been no Display Manager events recorded. If you think there should have been, check that the 'Glimpse for Orchard Content Manager' feature is enabled.";
            }

            return vm;
        }

        public override string Name
        {
            get { return "Content Manager"; }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<ContentManagerGetMessage>();
            context.PersistMessages<ContentManagerBuildDisplayMessage>();
        }

        public string Key
        {
            get { return "glimpse_orchard_contentmanager"; }
        }

        public bool KeysHeadings { get { return true; } }
    }
}