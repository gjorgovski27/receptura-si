using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CookingAssistantAPI.Models
{
    public class RecipeCreateModel
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public string Ingredients { get; set; } = null!;

        [Required]
        public string Instructions { get; set; } = null!;

        public int CookingTime { get; set; } // In minutes

        public int ServingSize { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }

        // Optional image file
        public IFormFile? ImageFile { get; set; }
    }
}
