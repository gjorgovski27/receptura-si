using CookingAssistantAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CookingAssistantAPI.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly CookingAssistantDbContext _context;

        public ProfileModel(CookingAssistantDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }

        public User User { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            User = await _context.Users.FirstOrDefaultAsync(u => u.UserId == UserId);

            if (User == null)
                return NotFound();

            return Page();
        }
    }
}
