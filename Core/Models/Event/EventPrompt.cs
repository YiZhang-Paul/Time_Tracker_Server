using Core.Enums;
using System;

namespace Core.Models.Event
{
    public class EventPrompt
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public PromptType PromptType { get; set; }
        public PromptConfirmType ConfirmType { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
