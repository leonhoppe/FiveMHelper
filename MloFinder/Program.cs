using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml;
using CarCombiner;
using CodeWalker.GameFiles;

namespace MloFinder {
    class Program {
        static void Main(string[] args) {
            string ymapFile = null;

            if (args.Length == 0) {
                ymapFile = Log.RequestInput("Specify the ymap file to find the MLO.");
            }
            else ymapFile = args[0];

            if (!File.Exists(ymapFile)) {
                Log.WriteError("The specified ymap file does not exist!");
                Console.ReadKey();
            }

            if (File.Exists(Path.GetTempPath() + "dlc.rpf"))
                File.Delete(Path.GetTempPath() + "dlc.rpf");
            var rpf = RpfFile.CreateNew(Path.GetTempPath(), "dlc.rpf");

            var manager = new RpfManager();
            manager.Init(new List<RpfFile> {rpf});

            byte[] data = File.ReadAllBytes(ymapFile);
            var file = RpfFile.CreateFile(rpf.Root, "mlo.ymap", data);
            data = rpf.ExtractFile(file);

            var xml = MetaXml.GetXml(file, data, out _);
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var entities = doc.GetElementsByTagName("entities")[0];

            List<Vector3d> positions = new List<Vector3d>();
            foreach (XmlNode entity in entities.ChildNodes) {
                var pos = entity.FindChild("position");
                if (pos == null) continue;
                
                var x = Convert.ToDouble(pos.Attributes.GetNamedItem("x").Value.Replace('.', ','));
                var y = Convert.ToDouble(pos.Attributes.GetNamedItem("y").Value.Replace('.', ','));
                var z = Convert.ToDouble(pos.Attributes.GetNamedItem("z").Value.Replace('.', ','));
                positions.Add(new Vector3d(x, y, z));
            }

            var decision = Log.RequestInput("How do you want to display the result?\n[1] Display all positions\n[2] Display the first 10 positions\n[3] Display the center of the positions");

            if (decision.Equals("1")) {
                Log.WriteLine(string.Join("\n", positions));
            }

            if (decision.Equals("2")) {
                if (positions.Count < 10) Log.WriteLine(string.Join("\n", positions));
                Log.WriteLine(string.Join("\n", positions.Take(10)));
            }

            if (decision.Equals("3")) {
                Log.WriteLine(CalculateCenterPoint(positions));
            }

            Console.ReadKey();
        }

        private static Vector3d CalculateCenterPoint(List<Vector3d> points) {
            double sx = 0;
            double sy = 0;
            double sz = 0;

            for (int i = 0; i < points.Count; i++) {
                sx += points[i].x;
                sy += points[i].y;
                sz += points[i].z;
            }

            double size = points.Count;
            return new Vector3d(sx / size, sy / size, sz / size);
        }
    }

    struct Vector3d {
        public double x;
        public double y;
        public double z;

        public Vector3d(double x, double y, double z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString() {
            return $"{x.ToString().Replace(',', '.')}, {y.ToString().Replace(',', '.')}, {z.ToString().Replace(',', '.')}";
        }
    }
}