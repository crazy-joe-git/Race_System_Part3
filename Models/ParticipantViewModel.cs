using System.Text.Json.Serialization;

namespace Race_System_2_.Models
{
    public class ParticipantViewModel
    {
        [JsonPropertyName("participantID")]
        public int ParticipantId { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = "Participant";
    }
}