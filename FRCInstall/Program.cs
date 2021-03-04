using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

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


        static XElement readXML(string url)
        {
            XElement doc = XElement.Load(url);
            return doc;
        }

        static void writeTextToDocument(string path, string content){
            string realPath = root + path;
            File.WriteAllText(realPath, content);
        }
        static string readDocument(string path)
        {
            string contents = File.ReadAllText(root+path);
            return contents;
        }

        /**
         * Eliminate a directory and its contents
         * 
         * Note that this uses an absolute path, rather than a relative like used elsewhere
         */
        static void eradicateDirectory(string path)
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

        static void initializePackageManager(string url)
        {
            writeTextToDocument(@"\origin.frcconfig", url);
            installUpdates();
        }

        static void installProgram(XElement program)
        {
            string type = (string)program.Element("Type");
            string name = (string)program.Element("Name");
            string fileName = (string)program.Element("FileName");
            Console.WriteLine(fileName);
        }

        static void installUpdates()
        {
            string origin = readDocument(@"\origin.frcconfig");

            XElement doc = readXML(origin);
            Console.WriteLine(doc);

            XElement[] programs = doc.Descendants("Program").ToArray();
            for(int i = 0; i < programs.Length; i++)
            {
                installProgram(programs[i]);
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
                Environment.Exit(0);
            }
            
            else if(args[0] == "init")
            {
                if(args.Length >= 2)
                {
                    eradicateDirectory(root);
                    System.IO.Directory.CreateDirectory(root);
                    System.IO.Directory.CreateDirectory(root + @"\entries");
                    System.IO.Directory.CreateDirectory(root + @"\temp");

                    initializePackageManager(args[1]);
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
