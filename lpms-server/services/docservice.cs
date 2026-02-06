
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LegalCaseManagement.Data;
using DocumentManagement.DTOs;
using LegalCaseManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Services
{
    /// <summary>
    /// Implements document business logic
    /// This file goes in: Services/DocumentService.cs
    /// </summary>
    public class DocumentService : IDocumentService
    {
        private readonly LegalCaseDbContext _context;
        private readonly IDocumentStorageService _storageService;
        private readonly IEncryptionService _encryptionService;

        public DocumentService(
            LegalCaseDbContext context,
            IDocumentStorageService storageService,
            IEncryptionService encryptionService)
        {
            _context = context;
            _storageService = storageService;
            _encryptionService = encryptionService;
        }

        public async Task<DocumentResponseDto> UploadDocumentAsync(byte[] fileData, DocumentUploadDto uploadDto)
        {
            var fileHash = _encryptionService.ComputeHash(fileData);
            var storagePath = await _storageService.SaveFileAsync(fileData, uploadDto.FileName!);

            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = uploadDto.FileName,
                FileExtension = Path.GetExtension(uploadDto.FileName),
                FileSizeBytes = fileData.Length,
                StoragePath = storagePath,
                Description = uploadDto.Description,
                UploadedBy = uploadDto.UploadedBy,
                UploadedDate = DateTime.UtcNow,
                FileHash = fileHash,
                DocumentType = uploadDto.DocumentType,
                AccessLevel = uploadDto.AccessLevel,
                IsArchived = false,
                IsEncrypted = false
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return MapToResponseDto(document);
        }

        public async Task<(byte[] fileData, string fileName)> RetrieveDocumentAsync(Guid documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            
            if (document == null)
            {
                throw new KeyNotFoundException($"Document with ID {documentId} not found");
            }
            var storagePath = document.IsArchived
                ? (document.EncryptedStoragePath ?? document.StoragePath ?? throw new InvalidOperationException("No storage path for document"))
                : (document.StoragePath ?? throw new InvalidOperationException("No storage path for document"));
            var fileData = await _storageService.RetrieveFileAsync(storagePath);

            // Decrypt if archived and encrypted
            if (document.IsArchived && document.IsEncrypted &&
                !string.IsNullOrWhiteSpace(document.EncryptionKey) &&
                !string.IsNullOrWhiteSpace(document.EncryptionIv))
            {
                var key = Convert.FromBase64String(document.EncryptionKey);
                var iv = Convert.FromBase64String(document.EncryptionIv);
                fileData = _encryptionService.DecryptFile(fileData, key, iv);
            }

            document.LastAccessedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (fileData, document.FileName ?? "UNkownFileName");
        }

        public async Task<List<DocumentResponseDto>> GetAllDocumentsAsync(bool includeArchived = false)
        {
            var query = _context.Documents.AsQueryable();

            if (!includeArchived)
            {
                query = query.Where(d => !d.IsArchived);
            }

            var documents = await query
                .OrderByDescending(d => d.UploadedDate)
                .ToListAsync();

            return documents.Select(MapToResponseDto).ToList();
        }

        public async Task<DocumentResponseDto> GetDocumentByIdAsync(Guid documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            
            if (document == null)
            {
                throw new KeyNotFoundException($"Document with ID {documentId} not found");
            }

            return MapToResponseDto(document);
        }

        public async Task<bool> ArchiveDocumentAsync(Guid documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            
            if (document == null)
            {
                return false;
            }

            // Read original file
            var originalPath = document.StoragePath ?? "UnknownPath";
            var originalName = document.FileName ?? "UnknownFileName";
            var originalBytes = await _storageService.RetrieveFileAsync(originalPath);

            // Encrypt bytes
            byte[] key;
            byte[] iv;
            var encryptedBytes = _encryptionService.EncryptFile(originalBytes, out key, out iv);

            // Save encrypted to archive
            var archivePath = await _storageService.SaveArchiveFileAsync(encryptedBytes, originalName);

            // Persist metadata
            document.IsArchived = true;
            document.ArchivedDate = DateTime.UtcNow;
            document.EncryptedStoragePath = archivePath;
            document.IsEncrypted = true;
            document.EncryptionKey = Convert.ToBase64String(key);
            document.EncryptionIv = Convert.ToBase64String(iv);

            // Optionally delete original file after archiving
            await _storageService.DeleteFileAsync(originalPath);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreDocumentAsync(Guid documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);

            if (document == null || !document.IsArchived)
            {
                return false;
            }

            // Determine archive path (encrypted or plain)
            var archivePath = document.EncryptedStoragePath ?? document.StoragePath;
            if (string.IsNullOrWhiteSpace(archivePath))
            {
                return false;
            }

            // Read archived bytes
            var archivedBytes = await _storageService.RetrieveFileAsync(archivePath);

            // Decrypt if necessary
            byte[] fileBytes = archivedBytes;
            if (document.IsEncrypted &&
                !string.IsNullOrWhiteSpace(document.EncryptionKey) &&
                !string.IsNullOrWhiteSpace(document.EncryptionIv))
            {
                var key = Convert.FromBase64String(document.EncryptionKey);
                var iv = Convert.FromBase64String(document.EncryptionIv);
                fileBytes = _encryptionService.DecryptFile(archivedBytes, key, iv);
            }

            // Save restored file to active storage
            var restoredPath = await _storageService.SaveFileAsync(fileBytes, document.FileName ?? "RestoredDocument");

            // Delete archived file
            await _storageService.DeleteFileAsync(archivePath);

            // Update document metadata
            document.StoragePath = restoredPath;
            document.IsArchived = false;
            document.ArchivedDate = null;
            document.EncryptedStoragePath = null;
            document.IsEncrypted = false;
            document.EncryptionKey = null;
            document.EncryptionIv = null;
            document.LastAccessedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDocumentAsync(Guid documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            
            if (document == null)
            {
                return false;
            }

            var storagePath = document.IsArchived ? document.EncryptedStoragePath : document.StoragePath;
            await _storageService.DeleteFileAsync(storagePath ?? "UnknownPath");

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return true;
        }

        private DocumentResponseDto MapToResponseDto(Document document)
        {
            return new DocumentResponseDto
            {
                Id = document.Id,
                FileName = document.FileName,
                FileExtension = document.FileExtension,
                FileSizeBytes = document.FileSizeBytes,
                Description = document.Description,
                UploadedBy = document.UploadedBy,
                UploadedDate = document.UploadedDate,
                IsArchived = document.IsArchived,
                DocumentType = document.DocumentType,
                AccessLevel = document.AccessLevel
            };
        }
    }
}
