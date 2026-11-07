using System.Text.Json.Serialization;

namespace Race_System_2_.Models
{
    public class RaceCategoryViewModel
    {
        [JsonPropertyName("categoryID")]
        public int CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("category_description")]
        public string CategoryDescription { get; set; } = string.Empty;
    }
}