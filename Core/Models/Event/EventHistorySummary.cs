using Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.Event
{
    public class EventHistorySummary
    {
        [Key]
        public long Id { get; set; }
        public long ResourceId { get; set; }
        public EventType EventType { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime Timestamp { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
}
