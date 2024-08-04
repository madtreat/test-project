namespace TestProject.Files {

    public class DirMetadata(string dirName) {
        public string DirName { get; set; } = dirName;
    }

    // should this really be a model?
    public class FileMoveOrCopy(string filePath, string newPath) {
        public string FilePath { get; set; } = filePath;
        public string NewPath { get; set; } = newPath;
    }

    public class FileDelete(string filePath) {
        public string FilePath { get; set; } = filePath;
    }

    public class FileMetadata(string filename, String contentType, long size, DateTime uploadDate) {
        public string FileName { get; set; } = filename;
        public string ContentType { get; set; } = contentType;
        public long Size { get; set; } = size;
        public DateTime UploadDate { get; set; } = uploadDate;
        public DateTime LastModifiedDate { get; set; } = uploadDate;

        public static string GetContentType(string path) {
            var types = new Dictionary<string, string> {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
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