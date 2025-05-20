using System.ComponentModel.DataAnnotations;

namespace CookingAssistantAPI.Models
{
    public class UserCreateModel
    {
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string UserFullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public string UserPassword { get; set; } = string.Empty;

        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only digits.")]
        public string UserPhone { get; set; } = string.Empty;
    }
}
