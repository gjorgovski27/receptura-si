using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookingAssistantAPI.Data
{
    public class Rating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RatingId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Value { get; set; } // Rating value between 1 and 5

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp for when the rating was created

        // Foreign key for the recipe
        [Required]
        public int RecipeId { get; set; }

        // Foreign key for the user who rated the recipe
        [Required]
        public int UserId { get; set; }

        // Navigation properties
        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; } = null!;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
