using System.ComponentModel.DataAnnotations;

namespace CookingAssistantAPI.Models
{
    public class FavouriteCreateModel
    {
        [Required]
        public int UserId { get; set; }  // The ID of the user marking the recipe as favourite

        [Required]
        public int RecipeId { get; set; }  // The ID of the recipe to be marked as favourite
    }
}
