using Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.Event
{
    public partial class EventHistory
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public long ResourceId { get; set; }
        [Required]
        public EventType EventType { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime Timestamp { get; set; }
    }
}
