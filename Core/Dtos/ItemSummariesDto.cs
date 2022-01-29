using System.Collections.Generic;

namespace Core.Dtos
{
    public class ItemSummariesDto<T>
    {
        public List<T> Resolved { get; set; } = new List<T>();
        public List<T> Unresolved { get; set; } = new List<T>();
    }
}
