using Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Core.Models.EventHistory
{
    public partial class EventHistory
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public long ResourceId { get; set; }
        [Required]
        public EventType EventType { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime Timestamp { get; set; }
    }
}
