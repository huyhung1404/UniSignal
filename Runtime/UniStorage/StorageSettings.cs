using UniCore.Attribute;
using UnityEngine;

namespace UniCore.Storage
{
    public interface ISettings
    {
        public int Version { get; }
        public ISerializer Serializer { get; }
        public IKey Key { get; }
        public IEncryptor Encryptor { get; }
        public IProtector Protector { get; }
        public IStorageProvider StorageProvider { get; }
    }

    public sealed class StorageSettings : ScriptableObject, ISettings
    {
        public int version;
        
        public SerializationType serializationType = SerializationType.Json;
        public InterfaceReference<ISerializer> serializerCustom;

        public KeyType keyType = KeyType.Static;
        public InterfaceReference<IKey> keyCustom;

        public EncryptionType encryptionType = EncryptionType.None;
        public InterfaceReference<IEncryptor> encryptorCustom;

        public ProtectorType protectorType = ProtectorType.None;
        public InterfaceReference<IProtector> protectorCustom;

        public StorageType storageType = StorageType.LocalStorage;
        public InterfaceReference<IStorageProvider> storageCustom;
        
        public int Version => version;

        public ISerializer Serializer
        {
            get
            {
                return serializationType switch
                {
                    SerializationType.Binary => new BinarySerializer(),
                    SerializationType.Custom => serializerCustom.Value,
                    _ => new JsonSerializer()
                };
            }
        }

        public IKey Key
        {
            get
            {
                return keyType switch
                {
                    KeyType.DeviceBoundKey => new DeviceBoundKey(),
                    KeyType.Custom => keyCustom.Value,
                    _ => new StaticKey()
                };
            }
        }

        public IEncryptor Encryptor
        {
            get
            {
                return encryptionType switch
                {
                    EncryptionType.AES => new AESEncryptor(),
                    EncryptionType.Custom => encryptorCustom.Value,
                    _ => new NoEncryptor()
                };
            }
        }

        public IProtector Protector
        {
            get
            {
                return protectorType switch
                {
                    ProtectorType.SHA256 => new SHA256Protector(),
                    ProtectorType.Custom => protectorCustom.Value,
                    _ => new NoProtector()
                };
            }
        }

        public IStorageProvider StorageProvider
        {
            get
            {
                return storageType switch
                {
                    StorageType.PlayerPrefs => new PlayerPrefsStorage(),
                    StorageType.Custom => storageCustom.Value,
                    _ => new LocalStorage()
                };
            }
        }
    }
}