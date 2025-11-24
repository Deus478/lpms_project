using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Official meeting minutes record (text summary, can link to a document)
    /// </summary>
    public class Minute
    {
        [Key]
        public int MinuteId { get; set; }

        [Required]
        public int MeetingId { get; set; }

        [Required]
        public DateTime RecordedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        public Guid? DocumentId { get; set; } // optional link to uploaded document

        [ForeignKey("MeetingId")]
        public virtual Meeting Meeting { get; set; } = null!;
    }
}


