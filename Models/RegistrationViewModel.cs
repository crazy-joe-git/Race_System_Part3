using System.Text.Json.Serialization;

namespace Race_System_2_.Models
{
    public class RegistrationViewModel
    {
        [JsonPropertyName("registrationID")]
        public int RegistrationId { get; set; }

        [JsonPropertyName("reg_Date")]
        public DateTime RegDate { get; set; }

        [JsonPropertyName("bidNumber")]
        public int BidNumber { get; set; }

        [JsonPropertyName("categoryID")]
        public int CategoryId { get; set; }

        [JsonPropertyName("paymentStatus")]
        public string PaymentStatus { get; set; } = "Pending";

        [JsonPropertyName("participantID")]
        public int ParticipantId { get; set; }

        [JsonPropertyName("eventRaceID")]
        public int EventRaceId { get; set; }
    }
}