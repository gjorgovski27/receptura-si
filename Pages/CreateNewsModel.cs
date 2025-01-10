using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CookingAssistantAPI.Models;
using CookingAssistantAPI.Data;
using System.IO;

namespace CookingAssistantAPI.Pages
{
    public class CreateNewsModel : PageModel
    {
        private readonly CookingAssistantDbContext _context;

        public CreateNewsModel(CookingAssistantDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required]
        [StringLength(255, ErrorMessage = "The description cannot exceed 255 characters.")]
        public string ArticleDescription { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        [StringLength(5000, ErrorMessage = "The content cannot exceed 5000 characters.")]
        public string ArticleContent { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        public IFormFile ArticleImage { get; set; } = null!;

        [BindProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Convert image to byte array
                using var memoryStream = new MemoryStream();
                await ArticleImage.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                // Create new News entity
                var news = new News
                {
                    ArticleDescription = ArticleDescription,
                    ArticleContent = ArticleContent,
                    ArticleImage = imageData,
                    ImageMimeType = ArticleImage.ContentType,
                    CreatedAt = DateTime.UtcNow
                };

                // Save the news to the database
                _context.News.Add(news);
                await _context.SaveChangesAsync();

                // Redirect back to the Dashboard
                return RedirectToPage("/Dashboard");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the news article. Please try again.");
                return Page();
            }
        }
    }
}
