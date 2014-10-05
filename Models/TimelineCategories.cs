using Glimpse.Core.Message;

namespace Glimpse.Orchard.Models
{
    internal static class TimelineCategories
    {
        public static TimelineCategoryItem Widgets = new TimelineCategoryItem("Widgets", "#3939AA", "#595980");
        public static TimelineCategoryItem Layers = new TimelineCategoryItem("Layers", "#AA3939", "#805959");
        public static TimelineCategoryItem Shapes = new TimelineCategoryItem("Shapes", "#39AA39", "#598059");
        public static TimelineCategoryItem Parts = new TimelineCategoryItem("Parts", "#AAAA39", "#808059");
        public static TimelineCategoryItem Fields = new TimelineCategoryItem("Fields", "#39AAAA", "#598080");
        public static TimelineCategoryItem ContentManagement = new TimelineCategoryItem("Content Manager", "#AA39AA", "#805980");
        public static TimelineCategoryItem Authorization = new TimelineCategoryItem("Authorization", "#393939", "#595959");
    }
}