using System;
using System.IO;

/// <summary>
/// Summary description for Class1
/// </summary>

namespace Kronox.Util
{
    public class FileManager
    {
        private string workDir;

        public FileManager()
        {

        }

        public FileManager(string workDir)
        {
            if (!Directory.Exists(workDir))
                Directory.CreateDirectory(workDir);

            this.workDir = workDir;
        }

        public bool checkIfExists(string path)
        {
            return File.Exists(path);
        }

        public void createFile(string fileName, string content)
        {
            File.WriteAllText(Path.Combine(this.workDir, fileName), content);
        }

        public void createFile(string fileName, byte[] content)
        {
            File.WriteAllBytes(Path.Combine(this.workDir, fileName), content);
        }

        public string getContentFile(string file, bool includePath = false)
        {
            return File.ReadAllText(includePath ? file : $"{this.workDir}\\{file}");
        }

        public byte[] getBytesContentFile(string file, bool includePath = false)
        {
            return File.ReadAllBytes(includePath ? file : $"{this.workDir}\\{file}");
        }

        public bool isEmptyFile(string file, bool includePath = false)
        {
            return this.getContentFile(file, includePath) == "";
        }

    }
}
