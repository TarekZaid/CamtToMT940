namespace CamtToMT940.Models
{
    public class FileEntry
    {
        public string FullPath { get; }

        public FileEntry(string fullPath)
        {
            FullPath = fullPath;
        }

        public override string ToString()
        {
            return System.IO.Path.GetFileName(FullPath);
        }
    }
}