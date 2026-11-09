using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Race_System_2_.Models
{
    [Table("Event")]
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Required]
        [Display(Name = "Location")]
        public string EventLocation { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Distance (km)")]
        public double Distance { get; set; }

        [Required]
        [Display(Name = "Event Type")]
        public string EventType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Organizer Name")]
        public string OrganizerName { get; set; } = string.Empty;

        [Display(Name = "Banner URL")]
        public string BannerUrl { get; set; } = string.Empty;
    }
}