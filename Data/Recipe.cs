using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

namespace CookingAssistantAPI.Data
{
    public class Recipe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecipeId { get; set; }

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

        // Foreign key for the user who created the recipe
        public int CreatedByUserId { get; set; }

        // Navigation property for the user who created the recipe
        [ForeignKey("CreatedByUserId")]
        public User? CreatedByUser { get; set; }

        // Optional image data (stored as a byte array)
        public byte[]? ImageData { get; set; }

        // MimeType of the image (e.g., "image/png", "image/jpeg")
        public string? ImageMimeType { get; set; }

        // Navigation property for ratings
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

        // Average rating - not stored in the database, but calculated when needed
        [NotMapped]
        public double AverageRating => Ratings.Count > 0 ? Ratings.Average(r => r.Value) : 0.0;

        // Navigation property for comments
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
