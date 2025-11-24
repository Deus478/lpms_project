using System;
using System.IO;
using System.Security.Cryptography;

namespace DocumentManagement.Services
{
    /// <summary>
    /// Implements encryption/decryption and hashing for documents
    /// This file goes in: Services/EncryptionService.cs
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        public byte[] EncryptFile(byte[] fileData, out byte[] key, out byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                aes.GenerateIV();
                
                key = aes.Key;
                iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(fileData, 0, fileData.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        public byte[] DecryptFile(byte[] encryptedData, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(encryptedData))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var resultStream = new MemoryStream())
                {
                    cs.CopyTo(resultStream);
                    return resultStream.ToArray();
                }
            }
        }

        public string ComputeHash(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(data);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
