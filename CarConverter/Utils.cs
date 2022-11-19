using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using RageLib.Archives;
using RageLib.GTA5.Archives;
using RageLib.GTA5.ArchiveWrappers;
using RageLib.GTA5.Cryptography;
using RageLib.GTA5.Utilities;

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

        /*public static void UnpackDlcFile(string dlcFile, string targetFolder, bool compress = true) {
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
        }*/

        public static void UnpackDlcFile(string dlcFile, string outFolder, bool compress = true) {
            try {
                var fileInfo = new FileInfo(dlcFile);
                if (Directory.Exists(outFolder))
                    Directory.Delete(outFolder, true);
                
                var fileStream = new FileStream(fileInfo.FullName, FileMode.Open);

                var inputArchive = RageArchiveWrapper7.Open(fileStream, fileInfo.Name);
                var queue = new List<Tuple<string, RageArchiveWrapper7, bool>>()
                    { new Tuple<string, RageArchiveWrapper7, bool>(fileInfo.FullName, inputArchive, false) };

                while (queue.Count > 0) {
                    var fullPath = queue[0].Item1;
                    var rpf = queue[0].Item2;
                    var isTmpStream = queue[0].Item3;
                
                    queue.RemoveAt(0);

                    ArchiveUtilities.ForEachFile(fullPath.Replace(fileInfo.FullName, ""), rpf.Root, rpf.archive_.Encryption, (fullFileName, file, encryption) => {
                        string path = outFolder + fullFileName;
                        string dir = Path.GetDirectoryName(path);

                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        Console.WriteLine(fullFileName);

                        if (file.Name.EndsWith(".rpf")) {
                            try {
                                var tmpStream = new FileStream(Path.GetTempFileName(), FileMode.Open);

                                file.Export(tmpStream);
                                RageArchiveWrapper7 archive = RageArchiveWrapper7.Open(tmpStream, file.Name);
                                queue.Add(new Tuple<string, RageArchiveWrapper7, bool>(fullFileName, archive, true));
                            }
                            catch (Exception e) {
                                Console.WriteLine(e);
                            }
                        }
                        else {
                            if (file.Name.EndsWith(".xml") || file.Name.EndsWith(".meta")) {
                                byte[] data = GetBinaryFileData((IArchiveBinaryFile)file, encryption);
                                string xml;

                                if (data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF) // Detect BOM
                                {
                                    xml = Encoding.UTF8.GetString(data, 3, data.Length - 3);
                                }
                                else {
                                    xml = Encoding.UTF8.GetString(data);
                                }

                                File.WriteAllText(path, xml, Encoding.UTF8);
                            }
                            else {
                                file.Export(path);
                                byte[] bytes = File.ReadAllBytes(path);
                                bytes[3] = 55;
                                File.WriteAllBytes(path, bytes);
                            }
                        }
                    });

                    var stream = (FileStream)rpf.archive_.BaseStream;
                    string fileName = stream.Name;

                    rpf.Dispose();

                    if (isTmpStream) {
                        File.Delete(fileName);
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
        
        private static byte[] GetBinaryFileData(IArchiveBinaryFile file, RageArchiveEncryption7 encryption)
        {
            using (var ms = new MemoryStream())
            {
                file.Export(ms);

                byte[] data = ms.ToArray();

                if(file.IsEncrypted)
                {
                    if (encryption == RageArchiveEncryption7.AES)
                    {
                        data = GTA5Crypto.DecryptAES(data);
                    }
                    else // if(encryption == RageArchiveEncryption7.NG)
                    {
                        data = GTA5Crypto.DecryptNG(data, file.Name, (uint)file.UncompressedSize);
                    }
                }

                if (file.IsCompressed)
                {
                    using (var dfls = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress))
                    {
                        using (var outstr = new MemoryStream())
                        {
                            dfls.CopyTo(outstr);
                            data = outstr.ToArray();
                        }
                    }
                }

                return data;
            }
        }

    }
}