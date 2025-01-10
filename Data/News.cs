using System;
using System.ComponentModel.DataAnnotations;

namespace CookingAssistantAPI.Data
{
    public class News
    {
        [Key]
        public int ArticleId { get; set; } // Unique primary key

        [Required]
        [MaxLength(255)]
        public string ArticleDescription { get; set; } = string.Empty; // Short description of the news article

        [Required]
        public string ArticleContent { get; set; } = string.Empty; // Detailed content of the news article

        public byte[]? ArticleImage { get; set; } // Image data for the article (stored as binary)

        [MaxLength(50)]
        public string? ImageMimeType { get; set; } // MIME type of the image (e.g., "image/jpeg")

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp when the article was posted
    }
}
