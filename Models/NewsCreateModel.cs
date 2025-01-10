using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CookingAssistantAPI.Models
{
    public class NewsCreateModel
    {
        [Required]
        [StringLength(255, ErrorMessage = "The description cannot exceed 255 characters.")]
        public string ArticleDescription { get; set; } = string.Empty;

        [Required]
        [StringLength(5000, ErrorMessage = "The content cannot exceed 5000 characters.")]
        public string ArticleContent { get; set; } = string.Empty;

        [Required]
        public IFormFile? ArticleImage { get; set; } // Uploaded image file, optional for cases without an image

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Automatically set timestamp
    }
}
