using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SE3E3
{
    class Program
    {
        static List<FileInfo> list;

        static void Main(string[] args)
        {
            string path = args[0];
            string nrOfFiles = args[1];
            Console.WriteLine("Path: {0}; Number of files: {1};", path, nrOfFiles);
            Stopwatch sw = new Stopwatch();
            list = new List<FileInfo>();
            sw.Start();
            PrintBiggestFiles(path, int.Parse(nrOfFiles));
            sw.Stop();
            //Not parallel
            TimeSpan ts = sw.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime SC " + elapsedTime);
            sw = new Stopwatch();
            list = new List<FileInfo>();
            sw.Start();
            list = PrintBiggestFilesParallel(path);
            list.AsParallel().OrderByDescending(f => f.Length).Take(int.Parse(nrOfFiles)).ForAll(fi => Console.WriteLine("File: {0}; Size: {1}", fi.FullName, fi.Length));
            sw.Stop();
            ts = sw.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime MC " + elapsedTime);
            Console.ReadLine();
        }

        private static List<FileInfo> PrintBiggestFilesParallel(string path)
        {
            List<FileInfo> fileInfoList = new List<FileInfo>();
            try
            {
                object monitor = new object();
                string[] filesPath = Directory.GetFiles(path);
                string[] foldersPath = Directory.GetDirectories(path);

                //foreach (string fp in foldersPath)
                //    PrintBiggestFiles(fp, null);
                Parallel.ForEach(
                    foldersPath,
                    (fp) => 
                    {
                        var aux = PrintBiggestFilesParallel(fp);
                        lock (monitor)
                        {
                            fileInfoList.AddRange(aux);
                        }
                    });

                //foreach (string fp in filesPath)
                //    list.Add(new FileInfo(fp));

                Parallel.ForEach(
                    filesPath,
                    () => new List<FileInfo>(),
                    (fp, pls, partialList) =>
                    {
                        FileInfo fileInfo = new FileInfo(fp);
                        partialList.Add(fileInfo);
                        return partialList;
                    },
                    (partialList) =>
                    {
                        lock (monitor)
                        {
                            fileInfoList.AddRange(partialList);
                        }
                    });
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Acesso negado: " + path);
            }

            return fileInfoList;

        }

        private static void PrintBiggestFiles(string path, int? nOfFiles)
        {
            try
            {
                string[] filesPath = Directory.GetFiles(path);
                string[] foldersPath = Directory.GetDirectories(path);

                foreach (string fp in foldersPath)
                    PrintBiggestFiles(fp, null);

                foreach (string fp in filesPath)
                { 
                    list.Add(new FileInfo(fp));
                }

                if (nOfFiles != null)
                {
                    List<FileInfo> sortedList = list.OrderBy(f => f.Length).Reverse().ToList();
                    for (int i = 0; i < nOfFiles; ++i)
                        Console.WriteLine("File: {0}; Size: {1}", sortedList[i].FullName, sortedList[i].Length);
                    Console.WriteLine("Files founded: " + sortedList.Count);
                }
            }
            catch(UnauthorizedAccessException)
            {
                Console.WriteLine("Acesso negado: " + path);
            }
        }



    }
}
