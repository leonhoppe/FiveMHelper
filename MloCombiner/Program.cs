using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CarCombiner;

namespace MloCombiner {
    class Program {
        public static void Main(string[] args) {
            var versionString = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            Console.Write(Constants.Motd);
            Console.WriteLine("\tv" + versionString);

            string rootFolder;
            string resourceName;
            string oldCombiner;

            if (args.Length == 0) {
                rootFolder = Log.RequestInput("Specify the root folder where the resources are located.");
                resourceName = Log.RequestInput("How sould the new resource be called?");
                
                string combineOld = Log.RequestInput("Would you like add a previously combined resource? (y/n)");
                if (combineOld.Equals("y"))
                    oldCombiner = Log.RequestInput("Specify the path of the previously combined resource.");
                else oldCombiner = null;

                BeginCombining(rootFolder, resourceName, oldCombiner);
                Console.ReadKey();
                return;
            }

            if (args.Contains("/?")) {
                Console.WriteLine("\nUsage:");
                Console.WriteLine("   mcb -> quick combine");

                Console.WriteLine("\nArguments:");
                Console.WriteLine("   -r <path> -> specify the source folder of the resources");
                Console.WriteLine("   -o <path> -> specify the output resource name");
                Console.WriteLine("   -c <path> -> include previously combined resources");
                Console.WriteLine();
                return;
            }

            if (!HandldeArgError(args)) return;

            rootFolder = args[Array.IndexOf(args, "-r") + 1];
            resourceName = args[Array.IndexOf(args, "-o") + 1];

            if (args.Contains("-c"))
                oldCombiner = args[Array.IndexOf(args, "-c") + 1];
            else oldCombiner = null;

            BeginCombining(rootFolder, resourceName, oldCombiner);
        }

        private static bool HandldeArgError(string[] args) {
            if (args.Count(arg => arg.Equals("-r")) > 1) {
                Log.WriteError("You can only combine resources from one source folder!");
                return false;
            }

            if (args.Count(arg => arg.Equals("-o")) > 1) {
                Log.WriteError("You can only create one output resource!");
                return false;
            }

            if (args.Count(arg => arg.Equals("-c")) > 1) {
                Log.WriteError("You can only include one previously combined resource!");
                return false;
            }

            if (!args.Contains("-r")) {
                Log.WriteError("you must specify a source folder!");
                return false;
            }

            if (!args.Contains("-o")) {
                Log.WriteError("you must specify a output resource!");
                return false;
            }

            return true;
        }

        private static void BeginCombining(string rootFolder, string resourceName, string oldCombiner) {
            Console.WriteLine();
            if (!Directory.Exists(rootFolder)) {
                Log.WriteError($"The folder '{rootFolder}' does not exist!");
                Environment.Exit(404);
                return;
            }

            if (oldCombiner != null && !Directory.Exists(oldCombiner)) {
                Log.WriteError($"The folder '{oldCombiner}' does not exist!");
                Environment.Exit(404);
                return;
            }

            Log.Write("Checking the resources... ");
            string[] extraFiles = Combiner.CheckMaps(rootFolder);
            Log.CompleteWrite(Constants.DoneString);

            Log.Write("Creating resource folder structure... ");
            string streamFolder = Combiner.CreateOutputResource(resourceName, rootFolder);
            Log.CompleteWrite(Constants.DoneString);

            Console.WriteLine();

            if (oldCombiner != null) {
                Log.Write("Copying old combined resource... ");
                Combiner.CopyOldCombiner(resourceName, oldCombiner);
                Log.CompleteWrite(Constants.DoneString);
            }

            Log.Write("Copying stream files to the new resource... ");
            Combiner.CopyStreamData(streamFolder, rootFolder);
            Log.CompleteWrite(Constants.DoneString);

            Console.WriteLine();

            Log.Write("Copying data_file entrys to the new resource...");
            Combiner.CopyDataFileEntrys(resourceName, rootFolder);
            Log.CompleteWrite(Constants.DoneString);

            Console.WriteLine();

            if (extraFiles.Length > 0) {
                Log.WriteWarning(
                    $"The following resources have extra metadata files who have not been copied:\n{string.Join(",\n", extraFiles)}\n");
            }

            Log.WriteLine("All resources have been merged!");
            Console.WriteLine();
        }
    }
}