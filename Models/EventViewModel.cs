using System;

namespace Race_System_2_.Models
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public string Location { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public int TotalEnrolments { get; set; }
        public string Status { get; set; } = "Active";
        public string BannerImageUrl { get; set; } = string.Empty;
    }
}