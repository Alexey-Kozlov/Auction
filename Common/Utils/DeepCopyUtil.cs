using System.Xml.Serialization;

namespace Common.Utils;

public class DeepCopyUtil
{
    public static void DeepCopy<T>(ref T object2Copy, ref T objectCopy)
    {
        using (var stream = new MemoryStream())
        {
            var serializer = new XmlSerializer(typeof(T));

            serializer.Serialize(stream, object2Copy);
            stream.Position = 0;
            objectCopy = (T)serializer.Deserialize(stream);
        }
    }
}