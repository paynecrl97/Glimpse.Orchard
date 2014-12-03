using Glimpse.Core.Message;
using Glimpse.Orchard.Models;

namespace Glimpse.Orchard.Extensions {
    public static class PerfmonCategoryExtensions 
    {
        public static TimelineCategoryItem ToGlimpseTimelineCategoryItem(this PerfmonCategory category)
        {
            return new TimelineCategoryItem(category.CategoryName, category.PrimaryColor, category.SecondaryColor);
        }
    }
}