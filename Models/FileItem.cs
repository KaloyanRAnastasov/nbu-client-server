namespace KaviClientServerProject.Models
{
    public class FileItem
    {
        public int Id { get; set; }
        public string FileName { get; set; } // Original file name
        public string FilePath { get; set; } // Path to the file on the server
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
