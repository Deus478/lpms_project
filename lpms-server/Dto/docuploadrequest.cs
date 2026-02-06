using Microsoft.AspNetCore.Http;

namespace DocumentManagement.DTOs
{
    /// <summary>
    /// DTO for handling multipart/form-data uploads from clients.
    /// </summary>
    public class DocumentUploadRequest
    {
        /// <summary>
        /// The file to upload.
        /// </summary>
        public IFormFile? File { get; set; }

        /// <summary>
        /// Optional title of the document.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Optional description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Client-provided category that maps to DocumentType in storage.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Access level classification (e.g., PUBLIC, PRIVATE).
        /// </summary>
        public string? AccessLevel { get; set; }
    }
}