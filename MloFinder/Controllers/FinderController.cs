using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeWalker.GameFiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MloFinder.Controllers {
    [ApiController]
    public class FinderController : Controller {

        private readonly ILogger<FinderController> _logger;

        public FinderController(ILogger<FinderController> logger) => _logger = logger;

        [HttpPost]
        [Route("unpack")]
        public async Task<IActionResult> GetLocation([FromQuery] string name) {
            try {
                if (!name.EndsWith("ymap"))
                    return Conflict("This api only accepts ymaps!");
            
                using var stream = new MemoryStream();
                await Request.Body.CopyToAsync(stream);
                byte[] data = stream.ToArray();

                var existingFiles = new List<RpfFileEntry>();
                Program.Rpf.ScanStructure(log => { }, log => _logger.LogError(log));
                Program.Rpf.GetFiles(Program.Rpf.Root, existingFiles, false);
                if (existingFiles.Any(f => f.Name == name))
                    RpfFile.DeleteEntry(existingFiles.SingleOrDefault(f => f.Name == name));
                var file = RpfFile.CreateFile(Program.Rpf.Root, name, data);
                data = Program.Rpf.ExtractFile(file);

                var xml = MetaXml.GetXml(file, data, out name);
                return Ok(xml);
            }
            catch (Exception e) {
                _logger.LogError(e, "Error tying to process file");
                throw;
            }
        }
    }
}