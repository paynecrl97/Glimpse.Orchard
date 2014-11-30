namespace Glimpse.Orchard.Models
{
    public class PerfmonCategory
    {
        public PerfmonCategory(string categoryName, string primaryColor, string secondaryColor)
        {
            CategoryName = categoryName;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
        }

        public string CategoryName { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
    }
}