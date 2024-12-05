using System;
using System.Collections.Generic; // Added to allow List<T>
using System.ComponentModel.DataAnnotations;
using CookingAssistantAPI.Data;


namespace CookingAssistantAPI.Data
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty; // Default value

        [Required]
        [MaxLength(100)]
        public string UserFullName { get; set; } = string.Empty; // Default value

        public DateTime UserBirthdate { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty; // Default value

        [Phone]
        public string UserPhone { get; set; } = string.Empty; // Default value

        [Required]
        public string UserPassword { get; set; } = string.Empty; // Default value

        // Ratings navigation property
        public List<Rating> Ratings { get; set; } = new List<Rating>();

        // Comments navigation property
        public List<Comment> Comments { get; set; } = new List<Comment>(); // Added to link user with their comments
    }
}
