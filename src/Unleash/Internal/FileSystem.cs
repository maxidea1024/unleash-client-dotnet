using System.IO;
using System.Text;

namespace Unleash.Internal
{
    internal class FileSystem : IFileSystem
    {
        private readonly Encoding encoding;

        public FileSystem(Encoding encoding)
        {
            this.encoding = encoding;
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
            File.WriteAllText(path, content, encoding);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path, encoding);
        }
    }
}
