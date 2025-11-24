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
        
        public bool IsEncrypted { get; set; }

        // Encryption metadata for archived documents
        public string ?EncryptionKey { get; set; }
        public string ?EncryptionIv { get; set; }
    }
}