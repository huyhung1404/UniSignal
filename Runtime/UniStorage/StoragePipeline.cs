using UnityEngine;

namespace UniCore.Storage
{
    public class StoragePipeline
    {
        public byte[] Key { get; private set; }
        private ISerializer serializer;
        private IEncryptor encryptor;
        private IProtector protector;
        private IStorageProvider storage;
        private int version;

        public StoragePipeline(ISettings settings)
        {
            if (settings == null)
            {
                LoadSettingDefault();
                return;
            }

            Key = settings.Key.GetKey();
            serializer = settings.Serializer;
            encryptor = settings.Encryptor;
            protector = settings.Protector;
            storage = settings.StorageProvider;
            version = settings.Version;
        }

        private void LoadSettingDefault()
        {
            Key = null;
            serializer = new JsonSerializer();
            encryptor = new NoEncryptor();
            protector = new NoProtector();
            storage = new LocalStorage();
        }

        public void Save<T>(string fileName, T data)
        {
            var bytes = Pack(data);
            storage.Save(fileName, bytes);
            PlayerPrefs.SetInt("storage_version", version);
            PlayerPrefs.Save();
        }

        public T Load<T>(string fileName)
        {
            var bytes = storage.Load(fileName);
            return bytes == null ? default : Unpack<T>(bytes);
        }

        public byte[] Pack<T>(T obj)
        {
            var raw = serializer.Serialize(obj);
            raw = encryptor.Encrypt(raw);
            return protector.Protect(raw);
        }

        public T Unpack<T>(byte[] data)
        {
            var raw = protector.Unprotect(data);
            raw = encryptor.Decrypt(raw);
            var result = serializer.Deserialize<T>(raw);
            var v = PlayerPrefs.GetInt("storage_version", version);
            if (v != version) StorageSystem.onVersionChanged?.Invoke(result, v, version);
            return result;
        }
    }
}