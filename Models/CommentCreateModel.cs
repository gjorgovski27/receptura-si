using System.ComponentModel.DataAnnotations;

namespace CookingAssistantAPI.Models
{
    public class CommentCreateModel
    {
        [Required]
        public int UserId { get; set; }  // ID of the user making the comment

        [Required]
        public int RecipeId { get; set; }  // ID of the recipe being commented on

        [Required]
        [MaxLength(1000, ErrorMessage = "Comment content cannot exceed 1000 characters.")]
        public string Content { get; set; } = string.Empty;  // The content of the comment with max length of 1000 characters
    }
}
