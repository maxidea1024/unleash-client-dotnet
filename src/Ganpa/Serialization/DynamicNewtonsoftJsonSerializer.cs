// TODO remove this file.
using System;
using System.IO;
using System.Text;

namespace Ganpa.Serialization
{
    internal class DynamicNewtonsoftJsonSerializer : IDynamicJsonSerializer
    {
        public string NugetPackageName => "Newtonsoft.Json (>= 9.0.1)";

        private readonly Encoding _encoding = new UTF8Encoding(false);

        private Type _jsonTextWriterType;
        private Type _jsonTextReaderType;

        private dynamic _serializer;

        public bool TryLoad()
        {
            var jsonSerializerType = Type.GetType("Newtonsoft.Json.JsonSerializer, Newtonsoft.Json");
            if (jsonSerializerType == null)
            {
                return false;
            }

            _serializer = Activator.CreateInstance(jsonSerializerType);

            var namingStrategyType =
                Type.GetType("Newtonsoft.Json.Serialization.CamelCaseNamingStrategy, Newtonsoft.Json");
            if (namingStrategyType == null)
            {
                return false;
            }

            dynamic namingStrategy = Activator.CreateInstance(namingStrategyType);
            namingStrategy.ProcessDictionaryKeys = false;

            var contractResolverType =
                Type.GetType("Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver, Newtonsoft.Json");
            dynamic contractResolver = Activator.CreateInstance(contractResolverType);
            contractResolver.NamingStrategy = namingStrategy;

            _serializer.ContractResolver = contractResolver;

            _jsonTextReaderType = Type.GetType("Newtonsoft.Json.JsonTextReader, Newtonsoft.Json");
            _jsonTextWriterType = Type.GetType("Newtonsoft.Json.JsonTextWriter, Newtonsoft.Json");

            return true;
        }

        public T Deserialize<T>(Stream stream)
        {
            using (var streamReader = new StreamReader(stream, _encoding))
            {
                dynamic textReader = Activator.CreateInstance(_jsonTextReaderType, streamReader);

                try
                {
                    return _serializer.Deserialize<T>(textReader);
                }
                finally
                {
                    (textReader as IDisposable)?.Dispose();
                }
            }
        }

        public void Serialize<T>(Stream stream, T instance)
        {
            // Default
            const int bufferSize = 1024 * 4;

            // Client code needs to dispose this.
            const bool leaveOpen = true;

            using (var writer = new StreamWriter(stream, _encoding, bufferSize, leaveOpen: leaveOpen))
            {
                dynamic jsonWriter = Activator.CreateInstance(_jsonTextWriterType, writer);

                try
                {
                    _serializer.Serialize(jsonWriter, instance);

                    jsonWriter.Flush();
                    stream.Position = 0;
                }
                finally
                {
                    (jsonWriter as IDisposable)?.Dispose();
                }
            }
        }
    }
}