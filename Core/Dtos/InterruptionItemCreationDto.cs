using Core.Enums;

namespace Core.Dtos
{
    public class InterruptionItemCreationDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
    }
}
