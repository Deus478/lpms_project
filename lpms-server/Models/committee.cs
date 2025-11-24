using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a committee that belongs to a Board
    /// </summary>
    public class Committee
    {
        [Key]
        public int CommitteeId { get; set; }

        [Required]
        public int BoardId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("BoardId")]
        public virtual Board Board { get; set; } = null!;

        // Navigation
        public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
    }
}


