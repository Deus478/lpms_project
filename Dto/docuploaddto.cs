using System.ComponentModel.DataAnnotations;

namespace DocumentManagement.DTOs
{
    /// <summary>
    /// DTO for document upload requests
    /// This file goes in: DTOs/DocumentUploadDto.cs
    /// Used by: POST /api/documents/upload
    /// </summary>
    public class DocumentUploadDto
    {
        [Required]
        public string ?FileName { get; set; }
        
        [MaxLength(500)]
        public string ?Description { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string ?DocumentType { get; set; }
        
        [Required]
        public string ?UploadedBy { get; set; }
    }
}
