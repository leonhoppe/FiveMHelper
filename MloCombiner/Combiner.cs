using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CarCombiner;

namespace MloCombiner {
    public class Combiner {
        public static string[] CheckMaps(string rootFolder) {
            var root = new DirectoryInfo(rootFolder);
            List<string> extraFiles = new();

            foreach (var resource in root.EnumerateDirectories()) {
                var extras = resource.EnumerateFiles()
                    .Where(info => !Constants.KnownMetaFiles.Contains(info.Name))
                    .ToArray();
                
                if (extras.Length != 0)
                    extraFiles.AddRange(extras.Select(info => $"[{resource.Name}] => {info.Name}"));
            }

            return extraFiles.ToArray();
        }

        public static string CreateOutputResource(string name, string rootFolder) {
            if (Directory.Exists(name)) {
                Log.WriteError("That folder already exist!");
                string delete = Log.RequestInput("Would you like to override the folder? (y/n)");
                if (delete.Equals("y"))
                    Directory.Delete(name, true);
                else
                    Environment.Exit(403);
            }
            
            DirectoryInfo info = Directory.CreateDirectory(name);
            
            File.WriteAllText(info.FullName + "/fxmanifest.lua", Constants.ManifestContent);

            var stream = info.CreateSubdirectory("stream");
            var root = new DirectoryInfo(rootFolder);
            foreach (var resource in root.EnumerateDirectories()) {
                stream.CreateSubdirectory('[' + resource.Name + ']');
            }

            return stream.FullName;
        }

        public static void CopyOldCombiner(string name, string oldCombiner) {
            var info = new DirectoryInfo(name);
            
            File.WriteAllText(info.FullName + "/fxmanifest.lua", File.ReadAllText(oldCombiner + "/fxmanifest.lua"));
            
            var oldStream = new DirectoryInfo(oldCombiner + "/stream");
            oldStream.CopyFilesRecursively(info.FullName + "/stream");
                
            foreach (var file in oldStream.Parent?.EnumerateFiles()) {
                if (file.FullName.EndsWith("fxmanifest.lua")) continue;
                file.CopyTo(info.FullName);
            }
        }

        public static void CopyStreamData(string streamFolder, string rootFolder) {
            DirectoryInfo resources = new DirectoryInfo(rootFolder);
            foreach (DirectoryInfo resource in resources.EnumerateDirectories()) {
                Log.Write("Copying " + resource.Name + "... ");

                string destFolder = streamFolder + Path.DirectorySeparatorChar + "[" + resource.Name + ']';
                resource.GetDirectories().Single(dir => dir.Name == "stream").CopyFilesRecursively(destFolder);
                foreach (var file in Directory.GetFiles(streamFolder + "/[" + resource.Name + ']', "_manifest.ymf", SearchOption.AllDirectories).Select(path => new FileInfo(path))) {
                    file.MoveTo(file.Directory?.FullName + Path.DirectorySeparatorChar + "_manifest_" + resource.Name + ".ymf");
                }
                
                Log.CompleteWrite(Constants.DoneString);
            }
        }

        public static void CopyDataFileEntrys(string name, string rootFolder) {
            DirectoryInfo resources = new DirectoryInfo(rootFolder);
            List<string> manifestLines = new List<string>();
            manifestLines.AddRange(File.ReadAllLines(name + "/fxmanifest.lua"));
            manifestLines.Add("");
            
            foreach (DirectoryInfo resource in resources.EnumerateDirectories()) {
                string manifest = resource.GetFiles().Any(file => file.FullName.EndsWith("fxmanifest.lua")) ? "fxmanifest.lua" : "__resource.lua";
                string[] lines = File.ReadAllLines(resource.FullName + Path.DirectorySeparatorChar + manifest);
                
                if (!lines.Any(line => line.StartsWith("data_file") && line.Contains("stream/"))) continue;

                lines = lines.Where(line => line.StartsWith("data_file")).ToArray();
                manifestLines.Add("");
                manifestLines.Add($"-- [{resource.Name}]");
                foreach (var line in lines) {
                    if (!line.Contains("stream/")) continue;
                    
                    manifestLines.Add(line.Replace("stream/", $"stream/[{resource.Name}]/"));
                }
            }
            
            File.WriteAllLines(name + "/fxmanifest.lua", manifestLines);
        }
    }
}