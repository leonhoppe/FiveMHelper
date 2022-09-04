using System;
using System.IO;
using CodeWalker.GameFiles;

namespace CarConverter {
    public static class Utils {

        public const string ManifestContent = @"
fx_version 'cerulean'
game 'gta5'
 
files {
    '*.meta'
}
 
data_file 'HANDLING_FILE' 'handling.meta'
data_file 'VEHICLE_LAYOUTS_FILE' 'vehiclelayouts.meta'
data_file 'VEHICLE_METADATA_FILE' 'vehicles.meta'
data_file 'CARCOLS_FILE' 'carcols.meta'
data_file 'VEHICLE_VARIATION_FILE' 'carvariations.meta'
";

        public static void UnpackDlcFile(string dlcFile, string targetFolder, bool compress = true) {
            string tempFolder = Path.GetTempPath() + Path.DirectorySeparatorChar + "MloConverter" +
                                Path.DirectorySeparatorChar + "OrigDlc";
            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);

            Directory.CreateDirectory(tempFolder);
            File.Copy(dlcFile, tempFolder + Path.DirectorySeparatorChar + "dlc.rpf");

            var manager = new RpfManager();
            manager.Init(Path.GetDirectoryName(tempFolder), status => Console.WriteLine("STATUS: " + status), error => Console.Error.WriteLine("ERROR: " + error));

            foreach (var rpf in manager.AllRpfs) {
                foreach (var entry in rpf.AllEntries) {
                    if (entry.Path.EndsWith(".rpf")) continue;
                    try {
                        var fentry = entry as RpfFileEntry;
                        if (fentry == null) continue;

                        byte[] data = entry.File.ExtractFile(fentry);

                        if (compress && !entry.Path.Contains("_manifest.ymf"))
                            data = ResourceBuilder.Compress(data);

                        var rrfe = fentry as RpfResourceFileEntry;
                        if (rrfe != null)
                            data = ResourceBuilder.AddResourceHeader(rrfe, data);

                        if (data != null) {
                            string outPath = targetFolder + entry.Path.Replace("origdlc" + Path.DirectorySeparatorChar + "dlc.rpf", "").Remove(0, 1);
                            string folder = Path.GetDirectoryName(outPath);

                            if (!Directory.Exists(folder))
                                Directory.CreateDirectory(folder);
                            
                            File.WriteAllBytes(outPath, data);
                        }
                    }
                    catch (Exception e) {
                        string err = entry.Name + ": " + e.Message;
                        manager.ErrorLog.Invoke(err);
                    }
                }
            }
            
            Directory.Delete(tempFolder, true);
        }
    }
}