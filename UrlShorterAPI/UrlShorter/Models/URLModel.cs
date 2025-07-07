
namespace UrlShorter.Models
{
    public class URLModel
    {
        public long? Id { get; set; }
        public string? OriginalUrl { get; set; }
        public string? ShortenedUrl { get; set; }
        public string? Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public User? User { get; set; }
    }
}
