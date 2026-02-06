using System;
using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.Models
{
   
    public class Document
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string ?FileName { get; set; }
        
        [Required]
        public string ?FileExtension { get; set; }
        
        public long FileSizeBytes { get; set; }
        
        [Required]
        public string ?StoragePath { get; set; }
        
        public string ?EncryptedStoragePath { get; set; }
        
        [MaxLength(500)]
        public string ?Description { get; set; }
        
        [Required]
        public string ?UploadedBy { get; set; }
        
        public DateTime UploadedDate { get; set; }
        
        public DateTime? LastAccessedDate { get; set; }
        
        public bool IsArchived { get; set; }
        
        public DateTime? ArchivedDate { get; set; }
        
        public string ?FileHash { get; set; }
        
        [MaxLength(100)]
        public string ?DocumentType { get; set; }
        
        [MaxLength(50)]
        public string ?AccessLevel { get; set; }
        
        public bool IsEncrypted { get; set; }

        // Enhanced classification fields
        [Required]
        [MaxLength(50)]
        public string OwnerDepartment { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ConfidentialityLevel { get; set; } = "Internal"; // Public, Internal, Confidential, Secret

        [Required]
        [MaxLength(50)]
        public string RetentionCategory { get; set; } = string.Empty; // Legal, Contract, Corporate, Memo, Correspondence

        public int RetentionYears { get; set; } = 7; // Default retention period

        public DateTime? RetentionExpiryDate { get; set; }

        public bool IsUnderLegalHold { get; set; } = false;

        public DateTime? LegalHoldDate { get; set; }

        [StringLength(500)]
        public string? LegalHoldReason { get; set; }

        public bool IsDisposed { get; set; } = false;

        public DateTime? DisposedDate { get; set; }

        public int? DisposedByUserId { get; set; }

        [StringLength(1000)]
        public string? DisposalReason { get; set; }

        // Metadata for Company Secretariat
        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(200)]
        public string? Subject { get; set; }

        public DateTime? DocumentDate { get; set; } // Actual date of the document content

        // Encryption metadata for archived documents
        public string ?EncryptionKey { get; set; }
        public string ?EncryptionIv { get; set; }
    }
}