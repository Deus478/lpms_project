
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagement.DTOs;

namespace DocumentManagement.Services
{
    /// <summary>
    /// Document business logic service interface
    /// This file goes in: Services/IDocumentService.cs
    /// </summary>
    public interface IDocumentService
    {
        Task<DocumentResponseDto> UploadDocumentAsync(byte[] fileData, DocumentUploadDto uploadDto);
        Task<(byte[] fileData, string fileName)> RetrieveDocumentAsync(Guid documentId);
        Task<List<DocumentResponseDto>> GetAllDocumentsAsync(bool includeArchived = false);
        Task<DocumentResponseDto> GetDocumentByIdAsync(Guid documentId);
        Task<bool> ArchiveDocumentAsync(Guid documentId);
        Task<bool> RestoreDocumentAsync(Guid documentId);
        Task<bool> DeleteDocumentAsync(Guid documentId);
    }
}