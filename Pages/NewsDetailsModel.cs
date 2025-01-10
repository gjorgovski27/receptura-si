using CookingAssistantAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace CookingAssistantAPI.Pages
{
    public class NewsDetailsModel : PageModel
    {
        private readonly CookingAssistantDbContext _context;

        public NewsDetailsModel(CookingAssistantDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int ArticleId { get; set; }

        public News News { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync()
        {
            // Fetch the news article by ID
            News = await _context.News.FindAsync(ArticleId);

            if (News == null)
            {
                return NotFound(); // Return 404 if the article does not exist
            }

            return Page();
        }
    }
}
