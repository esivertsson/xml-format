using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace XmlFormat
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PresentApplication();
                PrintUsage();
                return;
            }

            if (!IOHelper.IsThisAFile(args[0]))
            {
                Console.WriteLine("Cannot find file: " + Path.GetFullPath(args[0]));
                PrintUsage();
                return;
            }

            if (!IOHelper.CanWriteToDirectory(args[0]))
            {
                Console.WriteLine("Cannot write to directory: " + Path.GetFullPath(args[0]));
                PrintUsage();
                return;
            }

            try
            {
                string content = IOHelper.Read(args[0]);
                string formattedContent = XmlFormatter.Format(content, new FormatSettings());
                File.Copy(args[0], args[0] + ".bak", true);
                IOHelper.Write(args[0], formattedContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to parse XML: " + ex.Message);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: xmlformat [file name]");
            Console.WriteLine("\t [file name]: Should be a path to an XML-file");
            Console.WriteLine(@"Example: xmlformat C:\WebSite\web.config");
        }

        private static void PresentApplication()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.ProductVersion;
            Console.WriteLine("XmlFormat v." + version);
            Console.WriteLine("Formats a file containing XML-language. Copies the original to a .bak-file.");
        }
    }
}