using System.Text.Json.Serialization;

namespace Race_System_2_.Models
{
    public class EventViewModel
    {
        [JsonPropertyName("eventID")]
        public int Id { get; set; }

        [JsonPropertyName("eventName")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("eventDate")]
        public DateTime Date { get; set; }

        [JsonPropertyName("eventLocation")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("organizerName")]
        public string OrganizerName { get; set; } = string.Empty;
    }
}