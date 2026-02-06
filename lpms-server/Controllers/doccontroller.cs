using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using DocumentManagement.DTOs;
using DocumentManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace DocumentManagement.Controllers
{

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

        
        [HttpPost("upload")]
[RequestSizeLimit(52428800)] // 50MB
[Consumes("multipart/form-data")]
[ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadRequest request)
{
    try
    {
        // Validate file
        var file = request.File;
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
            Description = request.Description,
            DocumentType = request.Category ?? "General",
            AccessLevel = request.AccessLevel,
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

        
        
        [HttpGet]
        [ProducesResponseType(typeof(List<DocumentResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DocumentResponseDto>>> GetAllDocuments(
            [FromQuery] bool includeArchived = false)
        {
            var documents = await _documentService.GetAllDocumentsAsync(includeArchived);
            return Ok(documents);
        }

        
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

        [HttpPost("{id}/restore")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreDocument(Guid id)
        {
            var result = await _documentService.RestoreDocumentAsync(id);

            if (!result)
            {
                return NotFound($"Document with ID {id} not found");
            }

            return NoContent();
        }

      
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