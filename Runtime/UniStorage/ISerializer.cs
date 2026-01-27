using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;

namespace UniCore.Storage
{
    public enum SerializationType
    {
        Json,
        Binary,
        Custom
    }

    public interface ISerializer
    {
        public byte[] Serialize(object obj);
        public T Deserialize<T>(byte[] data);
    }

    public class JsonSerializer : ISerializer
    {
        public byte[] Serialize(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    public class BinarySerializer : ISerializer
    {
        public byte[] Serialize(object obj)
        {
            using var ms = new MemoryStream();
#pragma warning disable SYSLIB0011
            var bf = new BinaryFormatter();
            bf.Serialize(ms, obj);
#pragma warning restore SYSLIB0011
            return ms.ToArray();
        }

        public T Deserialize<T>(byte[] data)
        {
            using var ms = new MemoryStream(data);
#pragma warning disable SYSLIB0011
            var bf = new BinaryFormatter();
            return (T)bf.Deserialize(ms);
#pragma warning restore SYSLIB0011
        }
    }
}