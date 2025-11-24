using System.Threading.Tasks;

namespace DocumentManagement.Services
{
    /// <summary>
    /// Document storage service interface
    /// This file goes in: Services/IDocumentStorageService.cs
    /// </summary>
    public interface IDocumentStorageService
    {
        Task<string> SaveFileAsync(byte[] fileData, string fileName);
        Task<byte[]> RetrieveFileAsync(string storagePath);
        Task<bool> DeleteFileAsync(string storagePath);
        Task<string> ArchiveFileAsync(string storagePath, string fileName);
        Task<string> SaveArchiveFileAsync(byte[] fileData, string fileName);
    }
}