using Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Core.Models.ItemHistory
{
    public partial class ItemHistory
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public long ItemId { get; set; }
        [Required]
        public ItemType ItemType { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime Timestamp { get; set; }
    }
}
