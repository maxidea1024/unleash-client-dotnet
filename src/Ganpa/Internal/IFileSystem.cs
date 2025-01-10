using System.IO;

namespace Ganpa.Internal
{
    internal interface IFileSystem
    {
        bool FileExists(string path);
        
        Stream FileOpenRead(string path);
        
        Stream FileOpenCreate(string path);
        
        void WriteAllText(string path, string content);
 
        string ReadAllText(string path);
    }
}