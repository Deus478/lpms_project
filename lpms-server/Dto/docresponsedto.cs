using System;

namespace DocumentManagement.DTOs
{
    /// <summary>
    /// DTO for document responses (returned to client)
    /// This file goes in: DTOs/DocumentResponseDto.cs
    /// Used by: All GET endpoints and POST upload response
    /// </summary>
    public class DocumentResponseDto
    {
        public Guid Id { get; set; }
        public string  ?FileName { get; set; }
    
        public string ?FileExtension { get; set; }
        public long FileSizeBytes { get; set; }
        public string ?Description { get; set; }
        public string ?UploadedBy { get; set; }
        public DateTime UploadedDate { get; set; }
        public bool IsArchived { get; set; }
        public string? DocumentType { get; set; }
        public string? AccessLevel { get; set; }
   
    }
}
