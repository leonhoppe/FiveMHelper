using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BitComparer {
    class Program {
        static void Main(string[] args) {
            Console.Write("Select Mode [1/2]: ");
            
            int mode = Convert.ToInt32(Console.ReadLine());
            switch (mode) {
                case 1:
                    Mode1();
                    break;
                
                case 2:
                    Mode2();
                    break;
            }

            Console.ReadKey();
        }

        static void Mode1() {
            Console.Write("File 1: ");
            string file1 = Console.ReadLine();
            Console.Write("File 2: ");
            string file2 = Console.ReadLine();

            byte[] file1Bytes = File.ReadAllBytes(file1);
            byte[] file2Bytes = File.ReadAllBytes(file2);

            if (file1Bytes.Length != file2Bytes.Length) {
                Console.Error.WriteLine("The files does not have the same size");
                Environment.Exit(-1);
            }

            int byteCount = 0;
            for (int i = 0; i < file1Bytes.Length; i++) {
                if (file1Bytes[i] != file2Bytes[i]) {
                    Console.WriteLine($"Different byte at {i} File1: {file1Bytes[i]} File2: {file2Bytes[i]}");
                    byteCount++;
                }
            }

            Console.WriteLine("ByteCount: " + byteCount);
        }

        static void Mode2() {
            Console.Write("Directory 1: ");
            string dir1 = Console.ReadLine();
            Console.Write("Directory 2: ");
            string dir2 = Console.ReadLine();

            DirectoryInfo info = new DirectoryInfo(dir1);
            List<string> differentBytes = new();
            List<string> sameFiles = new();
            byte[] file1;
            byte[] file2;

            foreach (var file in info.EnumerateFiles()) {
                string path = dir2 + Path.DirectorySeparatorChar + file.Name;
                if (!File.Exists(path))
                    throw new NullReferenceException("File not present in second directory: " + file.Name);

                file1 = File.ReadAllBytes(file.FullName);
                file2 = File.ReadAllBytes(path);

                if (file1.Length != file2.Length)
                    throw new ThreadStateException("The files does not have the same size: " + file.Name);

                bool difference = false;
                for (int i = 0; i < file1.Length; i++) {
                    if (file1[i] != file2[i]) {
                        differentBytes.Add($"({file.Name}): [{i}] -> [{file1[i]}] - [{file2[i]}]");
                        difference = true;
                    }
                }
                
                if (!difference)
                    sameFiles.Add(file.Name);
            }

            Console.WriteLine("Different Bytes: \n" + string.Join("\n", differentBytes) + "\n\nSame Files:\n" + String.Join("\n", sameFiles));
        }
    }
}