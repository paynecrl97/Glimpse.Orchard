namespace Glimpse.Orchard.Models
{
    internal static class TimelineCategories
    {
        public static PerfmonCategory Widgets = new PerfmonCategory("Widgets", "#3939AA", "#595980");
        public static PerfmonCategory Layers = new PerfmonCategory("Layers", "#AA3939", "#805959");
        public static PerfmonCategory Shapes = new PerfmonCategory("Shapes", "#39AA39", "#598059");
        public static PerfmonCategory Fields = new PerfmonCategory("Fields", "#AAAA39", "#808059");
        public static PerfmonCategory Parts = new PerfmonCategory("Parts", "#FFA256", "#A65A1C");
        public static PerfmonCategory ContentManagement = new PerfmonCategory("Content Manager", "#AA39AA", "#805980");
        public static PerfmonCategory Authorization = new PerfmonCategory("Authorization", "#C50080", "#800053");
    }
}