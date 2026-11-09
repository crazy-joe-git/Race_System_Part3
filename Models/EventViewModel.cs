namespace Race_System_2_.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }
        public int Id
        {
            get => EventId;
            set => EventId = value;
        }

        public string EventName { get; set; } = string.Empty;
        public string Name
        {
            get => EventName;
            set => EventName = value;
        }

        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }
        public DateTime Date
        {
            get => EventDate;
            set => EventDate = value;
        }

        public string EventLocation { get; set; } = string.Empty;
        public string Location
        {
            get => EventLocation;
            set => EventLocation = value;
        }

        public double Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string OrganizerName { get; set; } = string.Empty;
        public string BannerUrl { get; set; } = string.Empty;
    }
}