using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

using TestProject.Files;

namespace TestProject.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class DirController : ControllerBase {

        private readonly ILogger<DirController> _logger;

        // TODO: make it user-specific?
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "FileStorage");


        public DirController(ILogger<DirController> logger) {
            _logger = logger;
            if (!Directory.Exists(_storagePath)) {
                Directory.CreateDirectory(_storagePath);
            }
        }

        [HttpGet]
        public IActionResult ListFiles() {
            var files = Directory.GetFiles(_storagePath);
            Console.WriteLine("\n--- dir: " + _storagePath);
            Console.WriteLine("\n--- files:\n" + files);
            return Ok();//ControllerContext.MyDisplayRouteInfo();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file) {
            if (file == null) {
                return BadRequest("You must select a file to upload");
            }
            if (file.Length == 0) {
                return BadRequest("Cannot upload an empty file");
            }

            var filePath = Path.Combine(_storagePath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create)) {
                await file.CopyToAsync(stream);
            }

            return Ok(new {
                file.FileName,
                file.ContentType,
                file.Length,
            });
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName) {
            var filePath = Path.Combine(_storagePath, fileName);

            if (!System.IO.File.Exists(filePath)) {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open)) {
                await stream.CopyToAsync(memory);
            }

            // reset current position in file
            memory.Position = 0;
            return File(memory, Files.FileMetadata.GetContentType(filePath), fileName);
        }
    }
}