namespace TestProject.Files {
    public class FileMetadata {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public FileMetadata(string filename, String contentType, long size, DateTime uploadDate) {
            FileName = filename;
            ContentType = contentType;
            Size = size;
            UploadDate = uploadDate;
            LastModifiedDate = uploadDate;
        }

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