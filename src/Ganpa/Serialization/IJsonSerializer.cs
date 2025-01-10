using System.IO;

namespace Ganpa.Serialization
{
    public interface IJsonSerializer
    {
        T Deserialize<T>(Stream stream);

        void Serialize<T>(Stream stream, T instance);
    }
}