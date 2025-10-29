
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DocumentManagement.Services
{
    /// <summary>
    /// Handles physical file storage operations
    /// This file goes in: Services/DocumentStorageService.cs
    /// </summary>
    public class DocumentStorageService : IDocumentStorageService
    {
        private readonly string _storagePath;
        private readonly string _archivePath;

        public DocumentStorageService(IConfiguration configuration)
        {
            _storagePath = configuration["DocumentStorage:BasePath"] ?? "Documents";
            _archivePath = configuration["DocumentStorage:ArchivePath"] ?? "Documents/Archive";
            
            Directory.CreateDirectory(_storagePath);
            Directory.CreateDirectory(_archivePath);
        }

        public async Task<string> SaveFileAsync(byte[] fileData, string fileName)
        {
            var sanitizedName = SanitizeFileName(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedName}";
            var fullPath = Path.Combine(_storagePath, uniqueFileName);
            
            await File.WriteAllBytesAsync(fullPath, fileData);
            return fullPath;
        }

        public async Task<byte[]> RetrieveFileAsync(string storagePath)
        {
            if (!File.Exists(storagePath))
            {
                throw new FileNotFoundException("Document not found", storagePath);
            }
            
            return await File.ReadAllBytesAsync(storagePath);
        }

        public async Task<bool> DeleteFileAsync(string storagePath)
        {
            if (File.Exists(storagePath))
            {
                await Task.Run(() => File.Delete(storagePath));
                return true;
            }
            return false;
        }

        public async Task<string> ArchiveFileAsync(string storagePath, string fileName)
        {
            if (!File.Exists(storagePath))
            {
                throw new FileNotFoundException("Document not found", storagePath);
            }

            var archiveFileName = $"{DateTime.UtcNow:yyyyMMdd}_{SanitizeFileName(fileName)}";
            var archiveFullPath = Path.Combine(_archivePath, archiveFileName);
            
            await Task.Run(() => File.Move(storagePath, archiveFullPath));
            return archiveFullPath;
        }

        public async Task<string> SaveArchiveFileAsync(byte[] fileData, string fileName)
        {
            var archiveFileName = $"{DateTime.UtcNow:yyyyMMdd}_{SanitizeFileName(fileName)}";
            var archiveFullPath = Path.Combine(_archivePath, archiveFileName);
            await File.WriteAllBytesAsync(archiveFullPath, fileData);
            return archiveFullPath;
        }

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "file";
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(fileName.Where(ch => !invalidChars.Contains(ch)).ToArray());
            cleaned = cleaned.Replace("..", string.Empty).Replace("\\", string.Empty).Replace("/", string.Empty);
            if (string.IsNullOrWhiteSpace(cleaned)) return "file";
            return cleaned.Length > 255 ? cleaned.Substring(0, 255) : cleaned;
        }
    }
}
