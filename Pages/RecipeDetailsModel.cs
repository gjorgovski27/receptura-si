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
        public int UserId { get; set; }

        public Recipe Recipe { get; set; } = null!;
        public double RecipeAverageRating { get; set; }
        public bool CanDelete { get; set; }
        public int? UserRating { get; set; }
        public List<Comment> RecipeComments { get; set; } = new List<Comment>();
        public bool IsFavourite { get; set; } // Indicates if the recipe is marked as a favourite by the user.

        [BindProperty]
        public string NewCommentContent { get; set; } = string.Empty;

        public async Task OnGetAsync(int recipeId, int userId)
        {
            RecipeId = recipeId;
            UserId = userId;

            Recipe = await _context.Recipes
                .Include(r => r.CreatedByUser)
                .Include(r => r.Ratings)
                .FirstOrDefaultAsync(r => r.RecipeId == RecipeId);

            if (Recipe != null)
            {
                RecipeAverageRating = Recipe.Ratings.Any()
                    ? Recipe.Ratings.Average(r => r.Value)
                    : 0.0;

                CanDelete = Recipe.CreatedByUserId == UserId;

                var userRating = Recipe.Ratings.FirstOrDefault(r => r.UserId == UserId);
                UserRating = userRating?.Value;

                RecipeComments = await _context.Comments
                    .Include(c => c.User)
                    .Where(c => c.RecipeId == RecipeId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                // Check if the recipe is already marked as a favourite by the user.
                IsFavourite = await _context.Favourites
                    .AnyAsync(f => f.RecipeId == RecipeId && f.UserId == UserId);
            }
        }

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

        [HttpPost]
        public async Task<IActionResult> OnPostSubmitRatingAsync(int ratingValue)
        {
            if (RecipeId == 0 || UserId == 0)
            {
                return BadRequest("Invalid RecipeId or UserId");
            }

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.RecipeId == RecipeId && r.UserId == UserId);

            if (existingRating != null)
            {
                existingRating.Value = ratingValue;
                _context.Ratings.Update(existingRating);
            }
            else
            {
                var newRating = new Rating
                {
                    RecipeId = RecipeId,
                    UserId = UserId,
                    Value = ratingValue
                };
                _context.Ratings.Add(newRating);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { recipeId = RecipeId, userId = UserId });
        }

        [HttpPost]
        public async Task<IActionResult> OnPostToggleFavouriteAsync()
        {
            if (RecipeId == 0 || UserId == 0)
            {
                return BadRequest("Invalid RecipeId or UserId");
            }

            var favourite = await _context.Favourites
                .FirstOrDefaultAsync(f => f.RecipeId == RecipeId && f.UserId == UserId);

            if (favourite != null)
            {
                // Remove from favourites if already marked
                _context.Favourites.Remove(favourite);
            }
            else
            {
                // Add to favourites
                var newFavourite = new Favourite
                {
                    RecipeId = RecipeId,
                    UserId = UserId
                };
                _context.Favourites.Add(newFavourite);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { recipeId = RecipeId, userId = UserId });
        }
    }
}
