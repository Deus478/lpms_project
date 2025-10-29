using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagement.DTOs;
using DocumentManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace DocumentManagement.Controllers
{
    /// <summary>
    /// API Controller for document management operations
    /// This file goes in: Controllers/DocumentsController.cs
    /// 
    /// AVAILABLE ENDPOINTS:
    /// ==================
    /// 1. POST   /api/documents/upload      - Upload a new document
    /// 2. GET    /api/documents/{id}        - Get document metadata by ID
    /// 3. GET    /api/documents/{id}/download - Download document file
    /// 4. GET    /api/documents             - Get all documents (with optional archived filter)
    /// 5. POST   /api/documents/{id}/archive  - Archive a document
    /// 6. DELETE /api/documents/{id}        - Delete a document permanently
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
          private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IDocumentService documentService , ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        /// <summary>
        /// Upload a new document
        /// Endpoint: POST /api/documents/upload
        /// </summary>
        /// <param name="request">A multipart/form-data request containing the file and its metadata.</param>
        /// <returns>Created document metadata</returns>
        [HttpPost("upload")]
[RequestSizeLimit(52428800)] // 50MB
[Consumes("multipart/form-data")]
[ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> UploadDocument(
    IFormFile file, 
    string? title, 
    string? description, 
    string? category)
{
    try
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        // Read file data
        byte[] fileData;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileData = ms.ToArray();
        }

        // Get user ID from claims
        var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value 
                     ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                     ?? "anonymous-user"; // Fallback for testing

        // Create upload DTO
        var uploadDto = new DocumentUploadDto
        {
            FileName = file.FileName,
            Description = description,
            DocumentType = category ?? "General",
            UploadedBy = userId
        };

        // Upload document
        var result = await _documentService.UploadDocumentAsync(fileData, uploadDto);
        
        return CreatedAtAction(
            nameof(GetDocument), 
            new { id = result.Id }, 
            result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error uploading document");
        return StatusCode(500, new { message = "Error uploading document", error = ex.Message });
    }
}

        /// <summary>
        /// Get document metadata by ID
        /// Endpoint: GET /api/documents/{id}
        /// </summary>
        /// <param name="id">Document GUID</param>
        /// <returns>Document metadata</returns>
        [HttpGet("{id}")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(lpms_server.Swagger.Examples.DocumentResponseDtoExample))]
        [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentResponseDto>> GetDocument(Guid id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                return Ok(document);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Document with ID {id} not found");
            }
        }

        /// <summary>
        /// Download document file
        /// Endpoint: GET /api/documents/{id}/download
        /// </summary>
        /// <param name="id">Document GUID</param>
        /// <returns>File download</returns>
        [HttpGet("{id}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadDocument(Guid id)
        {
            try
            {
                var (fileData, fileName) = await _documentService.RetrieveDocumentAsync(id);
                return File(fileData, "application/octet-stream", fileName);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Document with ID {id} not found");
            }
        }

        /// <summary>
        /// Get all documents with optional archived filter
        /// Endpoint: GET /api/documents?includeArchived=false
        /// </summary>
        /// <param name="includeArchived">Include archived documents (default: false)</param>
        /// <returns>List of documents</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<DocumentResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DocumentResponseDto>>> GetAllDocuments(
            [FromQuery] bool includeArchived = false)
        {
            var documents = await _documentService.GetAllDocumentsAsync(includeArchived);
            return Ok(documents);
        }

        /// <summary>
        /// Archive a document (moves to archive storage)
        /// Endpoint: POST /api/documents/{id}/archive
        /// </summary>
        /// <param name="id">Document GUID</param>
        /// <returns>No content on success</returns>
        [HttpPost("{id}/archive")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ArchiveDocument(Guid id)
        {
            var result = await _documentService.ArchiveDocumentAsync(id);
            
            if (!result)
            {
                return NotFound($"Document with ID {id} not found");
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a document permanently
        /// Endpoint: DELETE /api/documents/{id}
        /// </summary>
        /// <param name="id">Document GUID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            var result = await _documentService.DeleteDocumentAsync(id);
            
            if (!result)
            {
                return NotFound($"Document with ID {id} not found");
            }

            return NoContent();
        }
    }
}