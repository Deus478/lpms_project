using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a corporate board
    /// </summary>
    public class Board
    {
        [Key]
        public int BoardId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Committee> Committees { get; set; } = new List<Committee>();
        public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
    }
}


