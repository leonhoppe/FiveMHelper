using System.IO;

namespace MloCombiner {
    public static class DirectoryInfoExtensions {
        public static void CopyFilesRecursively(this DirectoryInfo sourcePath, string targetPath) {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath.FullName, "*", SearchOption.AllDirectories)) {
                Directory.CreateDirectory(dirPath.Replace(sourcePath.FullName, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath.FullName, "*.*", SearchOption.AllDirectories)) {
                File.Copy(newPath, newPath.Replace(sourcePath.FullName, targetPath), true);
            }
        }
    }
}