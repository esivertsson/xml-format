using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace XmlFormat
{
    public static class IOHelper
    {
        /// <summary>
        /// Reads all characters in a file.
        /// </summary>
        public static string Read(string file)
        {
            string content;
            using (TextReader reader = File.OpenText(file))
            {
                content = reader.ReadToEnd();
            }
            return content;
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, then closes the file. If the target already exists it is overwritten.
        /// </summary>
        public static void Write(string file, string fileContent)
        {
            File.WriteAllText(file, fileContent);
        }

        public static bool IsThisAFile(string path)
        {
            try
            {
                return File.Exists(path);
            }
            catch
            {
            }

            return false;
        }

        public static bool IsThisADirectory(string path)
        {
            try
            {
                return Directory.Exists(path);
            }
            catch
            {
            }

            return false;
        }

        public static bool CanWriteToDirectory(string fileOrDirectory)
        {
            string directory = Path.GetFullPath(fileOrDirectory);
            try
            {
                AuthorizationRuleCollection collection = Directory.GetAccessControl(directory)
                    .GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                if (collection.Cast<FileSystemAccessRule>().Any(rule => rule.AccessControlType == AccessControlType.Allow))
                {
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
