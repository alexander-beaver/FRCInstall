using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO.Compression;
using DiscUtils;
using DiscUtils.Iso9660;
using System.ComponentModel;

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

        private static void drawTextProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i <= onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;

                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 36;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }
        public static Boolean finishedDownload = false;
        public static int progress = 0;
        static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
          
           if (progress < e.ProgressPercentage)
           {
            progress = e.ProgressPercentage;
            drawTextProgressBar(progress, 100);
            }
        }

        static void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            finishedDownload = true;
        }
        static string DownloadTempFile(String url, String name)
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                if (finishedDownload)
                {
                    finishedDownload = false;
                }
                wc.DownloadFileTaskAsync(new Uri(url), root + @"\temp\" + name);
                while (true)
                {
                    if(finishedDownload)
                    {
                        finishedDownload = false;
                        progress = 0;
                        return root + @"\temp\" + name;
                    }
                }
            }
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
            if(type == "ISO")
            {
                string installerArguments = "/S";
                Console.WriteLine("downloading: " + name);
                String installerISO = DownloadTempFile(url, fileName);
                Console.WriteLine("finished downloading: " + name);
                using (FileStream isoStream = File.OpenRead(installerISO))
                {
                    CDReader cd = new CDReader(isoStream, true);
                    //Stream exe = cd.OpenFile(@"Folder\Hello.txt", FileMode.Open);
                    Console.WriteLine("extracting: " + name);
                    foreach (var file in cd.Root.GetFiles())
                    {
                        Console.WriteLine("extracting: " + file.Name);
                        //Stream fileStream = cd.OpenFile(file.Name, FileMode.Open);
                        //cd.CopyFile(file.Name, root + @"\temp\unzipped\" + name);
                        Stream fileStream = cd.OpenFile(file.Name, FileMode.Open);
                        using (FileStream Fs = File.Create(root + @"\temp\unzipped\" + name + "\\" + file.Name))
                        fileStream.CopyTo(Fs, 4 * 1024);
                    }
                    Console.WriteLine("\ninstalling: " + name);
                    String executable = root + @"\temp\unzipped\" + name + @"\" + (string)program.Element("ExecutableName");
                    var process = System.Diagnostics.Process.Start(executable, installerArguments);
                    process.WaitForExit();
                    Console.WriteLine("done installing: " + name);
                }
            }
            if (type == "EXE")
            {
                String executable = "";
                string installerArguments = "/S";
                Console.WriteLine("downloading: " + name);
                if (zip)
                {
                    Console.WriteLine("extracting: " + name);
                    String zippedEXE = DownloadTempFile(url, fileName);
                    ZipFile.ExtractToDirectory(zippedEXE, root + @"\temp\unzipped\" + name);
                    executable = root + @"\temp\unzipped\" + name + @"\" + (string)program.Element("ExecutableName");
                }
                else
                {
                    executable = DownloadTempFile(url, fileName);
                }
                Console.WriteLine("\ninstalling: " + name);
                var process = System.Diagnostics.Process.Start(executable, installerArguments);
                process.WaitForExit();
                Console.WriteLine("done installing: " + name);

            }
            if (type == "Asset")
            {
                Console.WriteLine("downloading: " + name);
                if (zip)
                {
                    String asset = DownloadTempFile(url, fileName);
                    ZipFile.ExtractToDirectory(asset, root + @"\" + name);
                }
                else
                {
                    String asset = DownloadTempFile(url, fileName);
                    System.IO.File.Copy(asset, root + @"\" + fileName, true);
                }
                Thread.Sleep(200);
                Console.WriteLine("\ndownloaded: " + fileName + " to C:\\Users\\Public\\Documents\\frcinstall");
            }


        }

        static void InstallUpdates()
        {
            string origin = ReadDocument(@"\origin.frcconfig");
            EradicateDirectory(root + @"\temp");
            System.IO.Directory.CreateDirectory(root + @"\temp");


            XElement doc = ReadXML(origin);
            Console.WriteLine("CLICKING THIS WINDOW BREAKS PROGRESS BAR, CTRL+C TO BRING IT BACK");

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
