using Microsoft.AspNetCore.Http.HttpResults;
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
            string[] files = Directory.GetFiles(_storagePath);
            string[] dirs = Directory.GetDirectories(_storagePath);
            Console.WriteLine("\n--- dir: " + _storagePath);
            Console.WriteLine("\n--- files:");

            List<string> paths = [];
            foreach (string file in files) {
                Console.WriteLine(file);
                paths.Add("file:" + file);
            }

            Console.WriteLine("\n--- dirs:");
            foreach (string dir in dirs) {
                Console.WriteLine(dir);
                paths.Add("dir:" + dir);
            }

            return Ok(paths);//ControllerContext.MyDisplayRouteInfo();
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

        [HttpPost("create")]
        public IActionResult MakeDirectory(DirMetadata data) {
            if (data == null) {
                return BadRequest("Must provide a valid directory name");
            }
            if (data.DirName.Length == 0) {
                return BadRequest("Must provide a non-zero directory name");
            }

            var fullPath = Path.Combine(_storagePath, data.DirName);
            Directory.CreateDirectory(fullPath);
            return Ok(new {
                data.DirName,
            });
        }

        [HttpPost("move")]
        public IActionResult MoveFile(FileMoveOrCopy data) {
            if (data == null) {
                return BadRequest("Must provide data for file move");
            }
            if (data.FilePath.Length == 0) {
                return BadRequest("Must provide a valid file path");
            }
            if (data.NewPath.Length == 0) {
                return BadRequest("Must provide a valid new file path");
            }

            var fullPathOld = Path.Combine(_storagePath, data.FilePath);
            var fullPathNew = Path.Combine(_storagePath, data.NewPath);

            Directory.Move(fullPathOld, fullPathNew);
            return Ok(new {
                data.NewPath,
            });
        }

        [HttpPost("copy")]
        public IActionResult CopyFile(FileMoveOrCopy data) {
            if (data == null) {
                return BadRequest("Must provide data for file move");
            }
            if (data.FilePath.Length == 0) {
                return BadRequest("Must provide a valid file path");
            }
            if (data.NewPath.Length == 0) {
                return BadRequest("Must provide a valid new file path");
            }

            var fullPathOld = Path.Combine(_storagePath, data.FilePath);
            var fullPathNew = Path.Combine(_storagePath, data.NewPath);

            System.IO.File.Copy(fullPathOld, fullPathNew);
            return Ok(new {
                data.NewPath,
            });
        }

        [HttpPost("delete")]
        public IActionResult DeleteFile(FileDelete data) {
            if (data == null) {
                return BadRequest("Must provide data for file delete");
            }
            if (data.FilePath.Length == 0) {
                return BadRequest("Must provide a valid file path");
            }

            var fileToBeDeleted = Path.Combine(_storagePath, data.FilePath);

            System.IO.File.Delete(fileToBeDeleted);
            return Ok(new {
                data.FilePath,
            });
        }
    }
}
