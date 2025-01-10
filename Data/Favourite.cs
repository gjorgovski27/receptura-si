using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookingAssistantAPI.Data
{
    public class Favourite
    {
        [Key]
        public int FavouriteId { get; set; } // Primary key for the action

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; } // ID of the user marking the recipe as favourite

        [Required]
        [ForeignKey("Recipe")]
        public int RecipeId { get; set; } // ID of the recipe being marked as favourite

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Recipe Recipe { get; set; } = null!;
    }
}
