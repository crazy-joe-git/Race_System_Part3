using System.Text.Json.Serialization;

namespace Race_System_2_.Models
{
    public class ResultViewModel
    {
        [JsonPropertyName("resultID")]
        public int ResultId { get; set; }

        [JsonPropertyName("finishTime")]
        public string FinishTime { get; set; } = string.Empty;

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("registrationID")]
        public int RegistrationId { get; set; }
    }
}