using System.Security.Cryptography;
using System;

namespace UniCore.Storage
{
    public enum ProtectorType
    {
        None,
        SHA256,
        Custom
    }

    public interface IProtector
    {
        public byte[] Protect(byte[] data);
        public byte[] Unprotect(byte[] data);
    }

    public class NoProtector : IProtector
    {
        public byte[] Protect(byte[] data) => data;
        public byte[] Unprotect(byte[] data) => data;
    }

    public class SHA256Protector : IProtector
    {
        public byte[] Protect(byte[] data)
        {
            var key = StorageSystem.GetKey();
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(data);
            var result = new byte[data.Length + hash.Length];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            Buffer.BlockCopy(hash, 0, result, data.Length, hash.Length);
            return result;
        }

        public byte[] Unprotect(byte[] data)
        {
            var key = StorageSystem.GetKey();
            var dataLen = data.Length - 32;

            var raw = new byte[dataLen];
            var hash = new byte[32];

            Buffer.BlockCopy(data, 0, raw, 0, dataLen);
            Buffer.BlockCopy(data, dataLen, hash, 0, 32);

            using var hmac = new HMACSHA256(key);
            var check = hmac.ComputeHash(raw);

            for (var i = 0; i < 32; i++)
                if (hash[i] != check[i])
                    throw new Exception("Save file modified!");

            return raw;
        }
    }
}