using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations; // Optional: for validation

namespace DocumentManagement.DTOs
{
    /// <summary>
    /// DTO for handling the incoming document upload request from a form.
    /// </summary>
    public class DocumentUploadRequest
    {
        /// <summary>
        /// The file to upload.
        /// </summary>
        [Required] // Optional: Add validation
        public IFormFile ?File { get; set; }

        /// <summary>
        /// Document description.
        /// </summary>
        [Required] // Optional: Add validation
        public string ?Description { get; set; }

        /// <summary>
        /// Type of document (e.g., "Contract", "Invoice").
        /// </summary>
        [Required] // Optional: Add validation
        public string ?DocumentType { get; set; }

        /// <summary>
        /// Username or ID of uploader.
        /// </summary>
        [Required] // Optional: Add validation
        public string ?UploadedBy { get; set; }
    }
}