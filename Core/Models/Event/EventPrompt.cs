using Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.Event
{
    public class EventPrompt
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public PromptType PromptType { get; set; }
        [Required]
        public PromptConfirmType ConfirmType { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime Timestamp { get; set; }
    }
}
