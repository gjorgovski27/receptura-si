using CookingAssistantAPI.Data;
using CookingAssistantAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CookingAssistantAPI.Pages
{
    public class HomePageModel : PageModel
    {
        private readonly CookingAssistantDbContext _context;

        public HomePageModel(CookingAssistantDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }

        // Properties to hold data for the Home page
        public string? UserName { get; set; } // Nullable to allow for initialization if user not found
        public List<Recipe> MyRecipes { get; set; } = new List<Recipe>();
        public List<Recipe> TopRatedRecipes { get; set; } = new List<Recipe>(); // List of top-rated recipes

        // OnGetAsync method to fetch user data, their recipes, and top-rated recipes based on userId
        public async Task OnGetAsync()
        {
            // Fetch the user's name
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == UserId);
            if (user != null)
            {
                UserName = user.UserFullName;  // Display the user's full name
                
                // Fetch recipes created by the user
                MyRecipes = await _context.Recipes
                    .Where(r => r.CreatedByUserId == UserId)
                    .ToListAsync();
            }

            // Fetch top-rated recipes (average rating >= 4)
            TopRatedRecipes = await _context.Recipes
                .Include(r => r.Ratings)
                .Where(r => r.Ratings.Any() && r.Ratings.Average(rt => rt.Value) >= 4)
                .ToListAsync();
        }
    }
}
