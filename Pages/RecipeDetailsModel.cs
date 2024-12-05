using CookingAssistantAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CookingAssistantAPI.Pages
{
    public class RecipeDetailsModel : PageModel
    {
        private readonly CookingAssistantDbContext _context;

        public RecipeDetailsModel(CookingAssistantDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int RecipeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; } // Get the logged-in UserId to determine access rights.

        public Recipe Recipe { get; set; } = null!;
        public double RecipeAverageRating { get; set; }
        public bool CanDelete { get; set; } // This property will determine if the user can delete the recipe.
        public int? UserRating { get; set; } // Stores the current user's rating for the recipe, if exists.
        public List<Comment> RecipeComments { get; set; } = new List<Comment>(); // List to hold comments for the recipe

        [BindProperty]
        public string NewCommentContent { get; set; } = string.Empty; // Property to store new comment content

        // OnGetAsync method to fetch recipe data and comments based on RecipeId
        public async Task OnGetAsync(int recipeId, int userId)
        {
            RecipeId = recipeId;
            UserId = userId;

            // Fetch the recipe, including the user who created it and ratings
            Recipe = await _context.Recipes
                .Include(r => r.CreatedByUser)
                .Include(r => r.Ratings) // Include Ratings to fetch user-specific ratings.
                .FirstOrDefaultAsync(r => r.RecipeId == RecipeId);

            if (Recipe != null)
            {
                // Calculate the average rating of the recipe
                RecipeAverageRating = Recipe.Ratings.Any()
                    ? Recipe.Ratings.Average(r => r.Value)
                    : 0.0;

                // Determine if the logged-in user can delete this recipe
                CanDelete = Recipe.CreatedByUserId == UserId;

                // Get the user's current rating if it exists
                var userRating = Recipe.Ratings.FirstOrDefault(r => r.UserId == UserId);
                UserRating = userRating?.Value;

                // Fetch the comments related to the recipe
                RecipeComments = await _context.Comments
                    .Include(c => c.User)
                    .Where(c => c.RecipeId == RecipeId)
                    .OrderByDescending(c => c.CreatedAt) // Order by latest comments first
                    .ToListAsync();
            }
        }

        // Handle a POST request for submitting a new comment
        [HttpPost]
        public async Task<IActionResult> OnPostSubmitCommentAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCommentContent))
            {
                ModelState.AddModelError(string.Empty, "Comment cannot be empty.");
                await OnGetAsync(RecipeId, UserId);
                return Page();
            }

            if (RecipeId == 0 || UserId == 0)
            {
                return BadRequest("Invalid RecipeId or UserId");
            }

            // Create a new comment
            var newComment = new Comment
            {
                RecipeId = RecipeId,
                UserId = UserId,
                Content = NewCommentContent,
                CreatedAt = System.DateTime.UtcNow
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { recipeId = RecipeId, userId = UserId });
        }

        // Handle a POST request for rating a recipe
        [HttpPost]
        public async Task<IActionResult> OnPostSubmitRatingAsync(int ratingValue)
        {
            if (RecipeId == 0 || UserId == 0)
            {
                return BadRequest("Invalid RecipeId or UserId");
            }

            // Fetch the existing rating if it exists
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.RecipeId == RecipeId && r.UserId == UserId);

            if (existingRating != null)
            {
                // Update existing rating value
                existingRating.Value = ratingValue;
                _context.Ratings.Update(existingRating);
            }
            else
            {
                // Create a new rating entry
                var newRating = new Rating
                {
                    RecipeId = RecipeId,
                    UserId = UserId,
                    Value = ratingValue
                };
                _context.Ratings.Add(newRating);
            }

            // Save the changes
            await _context.SaveChangesAsync();

            return RedirectToPage(new { recipeId = RecipeId, userId = UserId });
        }
    }
}
