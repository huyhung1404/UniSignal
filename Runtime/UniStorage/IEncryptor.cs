using System.Security.Cryptography;

namespace UniCore.Storage
{
    public enum EncryptionType
    {
        None,
        AES,
        Custom
    }

    public interface IEncryptor
    {
        public byte[] Encrypt(byte[] data);
        public byte[] Decrypt(byte[] data);
    }

    public class NoEncryptor : IEncryptor
    {
        public byte[] Encrypt(byte[] data) => data;
        public byte[] Decrypt(byte[] data) => data;
    }

    public class AESEncryptor : IEncryptor
    {
        public byte[] Encrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = StorageSystem.GetKey();
            aes.IV = new byte[16];

            using var enc = aes.CreateEncryptor();
            return enc.TransformFinalBlock(data, 0, data.Length);
        }

        public byte[] Decrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = StorageSystem.GetKey();
            aes.IV = new byte[16];

            using var dec = aes.CreateDecryptor();
            return dec.TransformFinalBlock(data, 0, data.Length);
        }
    }
}