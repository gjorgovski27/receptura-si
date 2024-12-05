using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CookingAssistantAPI.Data;

namespace CookingAssistantAPI.Data
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; } // Primary Key

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; } // Foreign Key to User

        [Required]
        [ForeignKey("Recipe")]
        public int RecipeId { get; set; } // Foreign Key to Recipe

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = null!; // The content of the comment, with a max length of 1000 chars

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp when the comment was created

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Recipe Recipe { get; set; } = null!;
    }
}
