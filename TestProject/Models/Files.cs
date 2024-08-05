namespace TestProject.Files {

    /*
     *  Directory list request body
     */
    public class DirListRequest(string? dirName) {
        public string DirName { get; set; } = dirName ?? "";
    }

    /*
     *  Directory list response body
     */
    public class ListDirResponse(string dirName, List<FileMetadata>? files, List<DirMetadata> dirs) {
        public string DirName { get; set; } = dirName;
        public List<FileMetadata> files { get; set; } = files ?? [];
        public List<DirMetadata> dirs { get; set; } = dirs ?? [];
    }

    public class DirMetadata(string dirName) {
        public string DirName { get; set; } = dirName;
    }

    /*
     *  Directory create request body
     */
    public class DirCreateRequest(string dirName) {
        public string DirName { get; set; } = dirName;
    }

    /*
     *  Directory create response body
     */
    public class CreateDirResponse(string dirName) {
        public string DirName { get; set; } = dirName;
    }

    /*
     *  File move and copy request body
     */
    public class FileMoveOrCopy(string filePath, string newPath) {
        public string FilePath { get; set; } = filePath;
        public string NewPath { get; set; } = newPath;
    }

    /*
     *  File move and copy response body
     */
    public class FileMoveOrCopyResponse(string filePath, string newPath) {
        public string FilePath { get; set; } = filePath;
        public string NewPath { get; set; } = newPath;
    }

    /*
     *  File delete request body
     */
    public class FileDelete(string filePath) {
        public string FilePath { get; set; } = filePath;
    }

    /*
     *  File delete response body
     */
    public class FileDeleteResponse(string filePath) {
        public string FilePath { get; set; } = filePath;
    }

    /*
     *  File download request body
     */
    public class FileDownload(string filePath) {
        public string FilePath { get; set; } = filePath;
    }

    /*
     *  File metadata for viewing and listing, etc. response body
     */
    public class FileMetadata(string filename, string contentType, long length, DateTime uploadDate, DateTime? lastModDate = null) {
        public string FileName { get; set; } = filename;
        public string ContentType { get; set; } = contentType;
        public long Length { get; set; } = length;
        public DateTime UploadDate { get; set; } = uploadDate;
        public DateTime ModifiedDate { get; set; } = lastModDate ?? uploadDate;

        public static string GetContentType(string path) {
            var types = new Dictionary<string, string> {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".zip", "application/x-zip-compressed"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }
    }
}