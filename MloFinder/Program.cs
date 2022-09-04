using System;
using System.Collections.Generic;
using System.IO;
using CodeWalker.GameFiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MloFinder {
    public class Program {
        public static RpfFile Rpf { get; private set; }
        public static RpfManager Manager { get; private set; }
        
        public static void Main(string[] args) {
            // Create temporarily dlc.rpf file
            if (File.Exists("dlc.rpf"))
                File.Delete("dlc.rpf");
            Rpf = RpfFile.CreateNew(Environment.CurrentDirectory, "dlc.rpf");
            Manager = new RpfManager();
            Manager.Init(new List<RpfFile> {Rpf});

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}