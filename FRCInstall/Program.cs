using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO.Compression;

/**
 * A program to process remote installation of FRC Tools
 * 
 * This program is not secure. It should be combined with other precautions
 */

namespace FRCInstall
{
    class Program
    {
        static string root = @"C:\Users\Public\Documents\frcinstall";

        /**
         * Reads an XML file from a file (absolute path) or url and returns it as an XElement
         */
        static XElement ReadXML(string url)
        {
            XElement doc = XElement.Load(url);
            return doc;
        }


        static void WriteTextToDocument(string path, string content){
            string realPath = root + path;
            File.WriteAllText(realPath, content);
        }
        static string ReadDocument(string path)
        {
            string contents = File.ReadAllText(root+path);
            return contents;
        }

        /**
         * Eliminate a directory and its contents
         * 
         * Note that this uses an absolute path, rather than a relative like used elsewhere
         */
        static void EradicateDirectory(string path)
        {

            if (System.IO.Directory.Exists(path))
            {
                Console.WriteLine("Eradicating Directory: " + path);
                System.IO.DirectoryInfo di = new DirectoryInfo(path);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
        }

        static void InitializePackageManager(string url)
        {
            WriteTextToDocument(@"\origin.frcconfig", url);
            InstallUpdates();
        }

        static string CreatePKGString(string year, string rev)
        {
            return year + "|" + rev;
        }

        static string DownloadTempFile(String url, String name)
        {
            WebClient w = new WebClient();
            w.DownloadFile(url, root + @"\temp\" + name);
            return root + @"\temp\" + name;
        }

        static void InstallProgram(XElement program)
        {
            string type = (string)program.Element("Type");
            string name = (string)program.Element("Name");
            string fileName = (string)program.Element("FileName");
            string year = (string)program.Element("Year");
            string rev = (string)program.Element("Revision");
            string url = (string)program.Element("URL");
            bool zip = (bool)program.Element("Zip");


            string entryPath = root + @"\entries\" + name + ".FRCPKG";

            if (File.Exists(entryPath))
            {
                string version = ReadDocument(entryPath);

                if (CreatePKGString(year, rev).Equals(version))
                {
                    return;

                }
                else
                {

                    
                }


            }
            if (type == "EXE")
            {
                if (zip)
                {
                    string execut = (string)program.Element("FileName");
                    Console.WriteLine("installing zipexe");
                    String zippedEXE = DownloadTempFile(url, fileName);
                    ZipFile.ExtractToDirectory(zippedEXE, root + @"\temp\unzipped\" + name);
                    String executable = root + @"\temp\unzipped\" + name + @"\" + (string)program.Element("ExecutableName");
                    Process.Start(executable);
                }
                else
                {
                    String executable = DownloadTempFile(url, fileName);
                    Process.Start(executable);
                }

            }
            if (type == "Asset")
            {
                if (zip)
                {
                    String asset = DownloadTempFile(url, fileName);
                    ZipFile.ExtractToDirectory(asset, root + @"\" + name);
                }
                else
                {
                    String asset = DownloadTempFile(url, fileName);
                    System.IO.File.Copy(asset, root + @"\" + name, true);
                }

            }


        }

        static void InstallUpdates()
        {
            string origin = ReadDocument(@"\origin.frcconfig");
            EradicateDirectory(root + @"\temp");
            System.IO.Directory.CreateDirectory(root + @"\temp");


            XElement doc = ReadXML(origin);
            Console.WriteLine(doc);

            XElement[] programs = doc.Descendants("Program").ToArray();
            for(int i = 0; i < programs.Length; i++)
            {
                InstallProgram(programs[i]);
            }
        }

     
        static void Main(string[] args)
        {
            Console.WriteLine("FRC Software Installer");
            Console.WriteLine("(c) 2021 Alex Beaver. All Rights Reserved.");

            if (args.Length == 0)
            {
                Console.WriteLine("Missing Arguments");
                Console.WriteLine("Run with `help` to get help");
                Environment.Exit(1);
            }
            if (args[0] == "help")
            {
                Console.WriteLine("Please run the powershell script instead of the exe directly");
                Console.WriteLine("If you meant to do this, add arguments for an output directory then a path to an xml file");
                Environment.Exit(0);
            }
            
            else if(args[0] == "init")
            {
                if(args.Length >= 2)
                {
                    EradicateDirectory(root);
                    System.IO.Directory.CreateDirectory(root);
                    System.IO.Directory.CreateDirectory(root + @"\entries");
                    System.IO.Directory.CreateDirectory(root + @"\temp");

                    Console.WriteLine(args[1]);

                    InitializePackageManager(args[1]);
                    Environment.Exit(0);
                    
                }
                else
                {
                    Console.WriteLine("Missing Arguments for Initializing");
                    Console.WriteLine("Run with `help` to get help");
                    Console.WriteLine(args.Length);
                    Environment.Exit(1);
                }

            }
        }
    }
}
