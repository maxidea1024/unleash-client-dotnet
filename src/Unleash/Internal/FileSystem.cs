using System.IO;
using System.Text;

namespace Unleash.Internal
{
    internal class FileSystem : IFileSystem
    {
        private readonly Encoding _encoding;

        public FileSystem(Encoding encoding)
        {
            _encoding = encoding;
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public Stream FileOpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public Stream FileOpenCreate(string path)
        {
            return File.Open(path, FileMode.Create);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content, _encoding);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path, _encoding);
        }
    }
}
