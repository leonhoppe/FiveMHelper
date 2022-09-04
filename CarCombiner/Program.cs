using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace CarCombiner {
    class Program {
        static void Main(string[] args) {
            Console.Title = "CarCombiner by Leon Hoppe";
            Console.WriteLine(Constants.Motd);
            
            string rootPath;

            if (args.Length == 1)
                rootPath = args[0];
            else
                rootPath = Log.RequestInput("Specify the root folder where the resources are located.");

            if (Directory.Exists(rootPath)) {
                Log.Write("Checking the resources... ");
                string[] extraFiles = CheckVehicles(rootPath);
                Log.CompleteWrite(Constants.DoneString);
                
                string folderName = Log.RequestInput("How sould the new resource be called?");
                
                Log.Write("Creating resource folder structure... ");
                string streamFolder = CreateOutputFolders(folderName);
                Log.CompleteWrite(Constants.DoneString);

                Console.WriteLine();
                
                Log.Write("Copying stream files to the new resource... ");
                CopyStreamData(streamFolder, rootPath);
                Log.CompleteWrite(Constants.DoneString);

                Console.WriteLine();
                
                Log.Write("Copying metadata files to the new resource... ");
                CopyMetadata(folderName, rootPath);
                Log.CompleteWrite(Constants.DoneString);

                Console.WriteLine();

                if (extraFiles.Length > 0) {
                    Log.WriteLine($"The following resources have extra metadata files who have not been copied:\n{string.Join(",\n", extraFiles)}\n");
                }
                
                Log.WriteLine("All resources have been merged!");
                Console.ReadKey();
            }
            else {
                Log.WriteError("That folder does not exist!");
                Environment.Exit(404);
            }
        }

        private static string[] CheckVehicles(string rootFolder) {
            var root = new DirectoryInfo(rootFolder);
            List<string> extraFiles = new();

            foreach (var resource in root.EnumerateDirectories()) {
                if (new DirectoryInfo(resource.FullName + Path.DirectorySeparatorChar + "stream")
                    .EnumerateFiles()
                    .Any(info => info.Name.Equals("_manifest.ymf"))) {
                    Log.WriteError($"Resource [{resource.Name}] contains mlo data thats not combinable!");
                    Environment.Exit(403);
                }

                var extras = resource.EnumerateFiles()
                    .Where(info => !Constants.KnownMetaFiles.Contains(info.Name))
                    .ToArray();
                
                if (extras.Length != 0)
                    extraFiles.AddRange(extras.Select(info => $"[{resource.Name}] => {info.Name}"));
            }

            return extraFiles.ToArray();
        }

        private static string CreateOutputFolders(string name) {
            if (Directory.Exists("./" + name)) {
                Log.WriteError("That folder already exist!");
                string delete = Log.RequestInput("Would you like to override the folder? (y/n)");
                if (delete.Equals("y"))
                    Directory.Delete("./" + name, true);
                else
                    Environment.Exit(403);
            }

            DirectoryInfo info = Directory.CreateDirectory("./" + name);
            File.WriteAllText(info.FullName + "/__resource.lua", Constants.ManifestContent);
            return info.CreateSubdirectory("stream").FullName;
        }

        private static void CopyStreamData(string streamFolder, string resourcesFolder) {
            DirectoryInfo resources = new DirectoryInfo(resourcesFolder);
            foreach (DirectoryInfo resource in resources.EnumerateDirectories()) {
                DirectoryInfo stream = new DirectoryInfo(resource.FullName + "/stream");
                
                Log.Write("Copying " + resource.Name + "... ");

                foreach (FileInfo file in stream.EnumerateFiles()) {
                    if (File.Exists(streamFolder + "/" + file.Name)) {
                        Log.WriteError("Stream resource [" + file.Name + "] already exist!");
                        Environment.Exit(409);
                    }

                    file.CopyTo(streamFolder + "/" + file.Name);
                }
                
                Log.CompleteWrite(Constants.DoneString);
            }
        }

        private static void CopyMetadata(string outPath, string resourcesRoot) {
            List<string> carcols = new List<string>();
            List<string> carvariations = new List<string>();
            List<string> dlctext = new List<string>();
            List<string> handling = new List<string>();
            List<string> vehicles = new List<string>();
            
            DirectoryInfo info = new DirectoryInfo(resourcesRoot);
            foreach (DirectoryInfo resource in info.EnumerateDirectories()) {
                string carcolsFile = resource.FullName + Path.DirectorySeparatorChar + "carcols.meta";
                string carvariationsFile = resource.FullName + Path.DirectorySeparatorChar + "carvariations.meta";
                string dlcFile = resource.FullName + Path.DirectorySeparatorChar + "dlctext.meta";
                string handlingFile = resource.FullName + Path.DirectorySeparatorChar + "handling.meta";
                string vehiclesFile = resource.FullName + Path.DirectorySeparatorChar + "vehicles.meta";

                if (File.Exists(carcolsFile)) carcols.Add(carcolsFile);
                if (File.Exists(carvariationsFile)) carvariations.Add(carvariationsFile);
                if (File.Exists(dlcFile)) dlctext.Add(dlcFile);
                if (File.Exists(handlingFile)) handling.Add(handlingFile);
                if (File.Exists(vehiclesFile)) vehicles.Add(vehiclesFile);
            }
            
            Log.Write("Combining carcols.meta... ");
            CombineCarcols(carcols).Save(outPath + "/carcols.meta");
            Log.CompleteWrite("no complications!");
            
            Log.Write("Combining carvariations.meta... ");
            CombineXmlDocuments(carvariations, "variationData").Save(outPath + "/carvariations.meta");
            Log.CompleteWrite("no complications!");
            
            Log.Write("Combining dlctext.meta... ");
            File.Copy(dlctext[0], outPath + "/dlctext.meta");
            Log.CompleteWrite("no complications!");
            
            Log.Write("Combining handling.meta... ");
            CombineXmlDocuments(handling, "HandlingData").Save(outPath + "/handling.meta");
            Log.CompleteWrite("no complications!");
            
            Log.Write("Combining vehicles.meta... ");
            CombineXmlDocuments(vehicles, "InitDatas", "txdRelationships").Save(outPath + "/vehicles.meta");
            Log.CompleteWrite("no complications!");
        }

        private static XmlDocument CombineXmlDocuments(List<string> files, params string[] rootNodes) {
            XmlDocument document = new XmlDocument();
            document.Load(files[0]);
            files.RemoveAt(0);
            
            foreach (string file in files) {
                try {
                    XmlDocument meta = new XmlDocument();
                    meta.Load(file);
                    
                    foreach (string rootNode in rootNodes) {
                        var node = document.ImportNode(meta.GetElementsByTagName(rootNode)[0].FirstChild, true);
                        document.GetElementsByTagName(rootNode)[0].AppendChild(node);
                    }
                }
                catch (Exception e) {
                    //Log.WriteError(e);
                    Log.WriteWarning("Could not combine [" + file + "] with other meta files: " + e.Message);
                }
            }

            return document;
        }

        private static XmlDocument CombineCarcols(List<string> files) {
            XmlDocument document = new XmlDocument();
            document.LoadXml(Constants.CarcolsScaffolding);
            
            foreach (string file in files) {
                try {
                    XmlDocument meta = new XmlDocument();
                    meta.Load(file);

                    foreach (var rootNode in new[] {"Kits", "Lights", "Sirens"}) {
                        var elements = meta.GetElementsByTagName(rootNode);
                        if (elements.Count == 0) continue;

                        var node = document.ImportNode(elements[0].FirstChild, true);
                        document.GetElementsByTagName(rootNode)[0].AppendChild(node);
                    }
                }
                catch (Exception e) {
                    //Log.WriteError(e);
                    Log.WriteWarning("Could not combine [" + file + "] with other meta files: " + e.Message);
                }
            }

            return document;
        }
    }
}