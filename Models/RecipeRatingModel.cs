using System.ComponentModel.DataAnnotations;

namespace CookingAssistantAPI.Models
{
    public class RatingCreateModel
    {
        [Required]
        public int UserId { get; set; }  // ID of the user who is giving the rating

        [Required]
        [Range(1, 5, ErrorMessage = "Rating value must be between 1 and 5.")]
        public int Value { get; set; }  // Rating value between 1 and 5

        [Required]
        public int RecipeId { get; set; }  // ID of the recipe being rated
    }
}
