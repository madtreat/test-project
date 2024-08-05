using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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

        /*
         *  Spent too long fighting the query parameters, since I am new to .net, so just making this a POST instead of a GET
         */
        // [HttpGet]
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ListFiles(DirListRequest? data) {
            string basePath = _storagePath;
            Console.WriteLine("query: " + data);
            if (data != null && !string.IsNullOrEmpty(data.DirName)) {
                basePath = Path.Combine(_storagePath, data.DirName.TrimStart('/'));
            }

            Console.WriteLine("retrieving base path: " + basePath);

            if (!Directory.Exists(basePath)) {
                string error = "Directory does not exist";
                return NotFound(new { error });
            }

            string[] filesRaw = Directory.GetFiles(basePath);
            string[] dirsRaw = Directory.GetDirectories(basePath);

            List<FileMetadata> files = [];
            foreach (string filePath in filesRaw) {
                Console.WriteLine();
                var fileInfo = new System.IO.FileInfo(filePath);

                var file = new FileMetadata(
                    fileInfo.Name.TrimStart('/'),
                    // FileMetadata.GetContentType(filePath),
                    "some type",
                    fileInfo.Length,
                    fileInfo.CreationTime,
                    fileInfo.LastWriteTime
                );
                files.Add(file);
            }

            List<DirMetadata> dirs = [];
            foreach (string dirPath in dirsRaw) {
                var dirInfo = new DirectoryInfo(dirPath);
                var dir = new DirMetadata(
                    dirInfo.Name.TrimStart('/')
                );
                dirs.Add(dir);
            }

            string dirName = data == null ? "" : data.DirName;
            return Ok(new ListDirResponse(dirName, files, dirs));
        }


        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // [ProducesResponseType(StatusCodes.Status201Created)] // TODO: use me instead of 200?
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string? dir) {
            if (file == null) {
                return BadRequest("You must select a file to upload");
            }
            if (file.Length == 0) {
                return BadRequest("Cannot upload an empty file");
            }

            var dirPath = _storagePath;
            if (dir != null) {
                dirPath = Path.Combine(_storagePath, dir.TrimStart('/'));
                if (!Directory.Exists(dirPath)) {
                    string error = "Directory does not exist: " + dir;
                    return BadRequest(new { error });
                }
            }

            var filePath = Path.Combine(dirPath, file.FileName);

            if (System.IO.File.Exists(filePath)) {
                // Two options here:
                // 1. create a "copy of [filename]" file, so they never collide, which takes a lot more validation
                //    and looping to ensure increasing copy values to never have collissions ever, or
                // 2. just take the quick route and throw an error, making the client rename the file
                // I choose option 2 for this exercise
                string error = "Cannot overwrite an existing file; please choose another name";
                return BadRequest(new {
                    error
                });
            }

            using (var stream = new FileStream(filePath, FileMode.Create)) {
                await file.CopyToAsync(stream);
            }

            System.Console.WriteLine("UploadFile file returning...");
            return Ok(new FileMetadata(
                file.FileName,
                file.ContentType,
                file.Length,
                DateTime.Now
            ));
        }


        // TODO: does not work with files inside dirs
        // [HttpGet("download")]
        [HttpPost("download")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadFile(FileDownload data) {
            if (data == null) {
                return BadRequest("Must provide data for file delete");
            }
            if (data.FilePath.Length == 0) {
                return BadRequest("Must provide a valid file path");
            }

            var filePath = Path.Combine(_storagePath, data.FilePath);

            if (!System.IO.File.Exists(filePath)) {
                string error = "File not found: " + data.FilePath;
                return NotFound(new { error });
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open)) {
                await stream.CopyToAsync(memory);
            }

            // reset current position in file
            memory.Position = 0;
            return File(memory, FileMetadata.GetContentType(filePath), data.FilePath);
        }


        [HttpPost("create")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult MakeDirectory(DirCreateRequest data) {
            if (data == null) {
                return BadRequest("Must provide a valid directory name");
            }
            if (data.DirName.Length == 0) {
                return BadRequest("Must provide a non-zero directory name");
            }

            var fullPath = Path.Combine(_storagePath, data.DirName);

            // ensure idempotent operations
            if (Directory.Exists(fullPath)) {
                string warning = "Directory already exists: " + data.DirName;
                return Ok(new {
                    warning,
                });
            }

            // this creates nested directories if necessary
            Directory.CreateDirectory(fullPath);
            return Ok(new CreateDirResponse(data.DirName));
        }


        [HttpPost("move")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

            var fullPathOld = Path.Combine(_storagePath, data.FilePath.TrimStart('/'));
            var fullPathNew = Path.Combine(_storagePath, data.NewPath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPathOld)) {
                Console.WriteLine("moving file error DNE: " + fullPathOld);
                return NotFound("File does not exist: " + data.FilePath);
            }

            if (System.IO.File.Exists(fullPathNew)) {
                // Two options here:
                // 1. create a "copy of [filename]" file, so they never collide, which takes a lot more validation
                //    and looping to ensure increasing copy values to never have collissions ever, or
                // 2. just take the quick route and throw an error, making the client rename the file
                // I choose option 2 for this exercise
                return BadRequest("Cannot overwrite an existing file; please choose another name: " + data.NewPath);
            }

            Directory.Move(fullPathOld, fullPathNew);
            return Ok(new FileMoveOrCopyResponse(fullPathOld, fullPathNew));
        }


        [HttpPost("copy")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

            var fullPathOld = Path.Combine(_storagePath, data.FilePath.TrimStart('/'));
            var fullPathNew = Path.Combine(_storagePath, data.NewPath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPathOld)) {
                return NotFound("File does not exist: " + data.FilePath);
            }

            if (System.IO.File.Exists(fullPathNew)) {
                // Two options here:
                // 1. create a "copy of [filename]" file, so they never collide, which takes a lot more validation
                //    and looping to ensure increasing copy values to never have collissions ever, or
                // 2. just take the quick route and throw an error, making the client rename the file
                // I choose option 2 for this exercise
                return BadRequest("Cannot overwrite an existing file; please choose another name: " + data.NewPath);
            }

            System.IO.File.Copy(fullPathOld, fullPathNew);
            return Ok(new FileMoveOrCopyResponse(fullPathOld, fullPathNew));
        }


        [HttpDelete("delete")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteFile(FileDelete data) {
            Console.WriteLine("Delete called for file: " + data);
            if (data == null) {
                return BadRequest("Must provide data for file delete");
            }
            if (data.FilePath.Length == 0) {
                return BadRequest("Must provide a valid file path");
            }

            var fileToBeDeleted = Path.Combine(_storagePath, data.FilePath);

            if (!System.IO.File.Exists(fileToBeDeleted)) {
                string error = "File does not exist: " + data.FilePath;
                return NotFound(new { error });
            }

            System.IO.File.Delete(fileToBeDeleted);
            return Ok(new FileDeleteResponse(data.FilePath));
        }
    }
}
