using System.Text;
using UnityEngine;

namespace UniCore.Storage
{
    public enum KeyType
    {
        Static,
        DeviceBoundKey,
        Custom
    }

    public interface IKey
    {
        public byte[] GetKey();
    }

    public class StaticKey : IKey
    {
        public byte[] GetKey()
        {
            var seed = Application.identifier + "UniStorageStatic";
            return SHA256(Encoding.UTF8.GetBytes(seed));
        }

        public static byte[] SHA256(byte[] data)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            return sha.ComputeHash(data);
        }
    }

    public class DeviceBoundKey : IKey
    {
        public byte[] GetKey()
        {
            var seed = SystemInfo.deviceUniqueIdentifier + Application.identifier;
            return SHA256(Encoding.UTF8.GetBytes(seed));
        }

        public static byte[] SHA256(byte[] data)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            return sha.ComputeHash(data);
        }
    }
}