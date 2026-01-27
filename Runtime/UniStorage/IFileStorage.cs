using System;
using System.IO;
using UnityEngine;

namespace UniCore.Storage
{
    public enum StorageType
    {
        LocalStorage,
        PlayerPrefs,
        Custom
    }

    public interface IStorageProvider
    {
        public void Save(string fileName, byte[] data);
        public byte[] Load(string fileName);
    }

    public class PlayerPrefsStorage : IStorageProvider
    {
        private static string Key(string fileName) => $"storage_{fileName}";

        public void Save(string fileName, byte[] data)
        {
            var base64 = Convert.ToBase64String(data);
            PlayerPrefs.SetString(Key(fileName), base64);
            PlayerPrefs.Save();
        }

        public byte[] Load(string fileName)
        {
            if (!PlayerPrefs.HasKey(Key(fileName))) return null;
            var base64 = PlayerPrefs.GetString(Key(fileName));
            return Convert.FromBase64String(base64);
        }
    }

    public class LocalStorage : IStorageProvider
    {
        private static string GetPath(string fileName) => Path.Combine(Application.persistentDataPath, $"{fileName}.dat");

        public void Save(string fileName, byte[] data) => File.WriteAllBytes(GetPath(fileName), data);

        public byte[] Load(string fileName) => File.Exists(GetPath(fileName)) ? File.ReadAllBytes(GetPath(fileName)) : null;
    }
}