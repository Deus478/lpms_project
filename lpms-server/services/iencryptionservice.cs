namespace DocumentManagement.Services
{
    /// <summary>
    /// Encryption service interface
    /// This file goes in: Services/IEncryptionService.cs
    /// </summary>
    public interface IEncryptionService
    {
        byte[] EncryptFile(byte[] fileData, out byte[] key, out byte[] iv);
        byte[] DecryptFile(byte[] encryptedData, byte[] key, byte[] iv);
        string ComputeHash(byte[] data);
    }
}