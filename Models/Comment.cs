namespace KaviClientServerProject.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public string? UserId { get; set; }
        public required string UserName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
