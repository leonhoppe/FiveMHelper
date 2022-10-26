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
            manager.Init(tempFolder, Console.WriteLine, Console.Error.WriteLine);
            foreach (var rpf in manager.AllRpfs) {
                foreach (var entry in rpf.AllEntries) {
                    try {
                        var fentry = entry as RpfFileEntry;
                        if (fentry == null) continue;

                        byte[] data = entry.File.ExtractFile(fentry);

                        var rrfe = fentry as RpfResourceFileEntry;
                        if (rrfe != null) {
                            if (compress) {
                                data = ResourceBuilder.Compress(data);
                            }
                            
                            data = ResourceBuilder.AddResourceHeader(rrfe, data);
                        }

                        if (data != null) {
                            var info = new FileInfo(entry.Name);
                            string bpath = targetFolder + "\\" + entry.Path.Substring(0, entry.Path.Length - info.Extension.Length);
                            string fpath = bpath + info.Extension;
                            int c = 1;
                            while (File.Exists(fpath)) {
                                fpath = bpath + "_" + c + info.Extension;
                                c++;
                            }

                            string rfolder = Path.GetDirectoryName(fpath);
                            if (fpath.EndsWith(".rpf")) {
                                Directory.CreateDirectory(fpath);
                                continue;
                            }
                            
                            if (!Directory.Exists(rfolder))
                                Directory.CreateDirectory(rfolder);
                            File.WriteAllBytes(fpath, data);
                        }
                        else {
                            throw new NullReferenceException("File data is null");
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            Directory.Delete(tempFolder, true);
        }
    }
}