using CookingAssistantAPI.Data;
using CookingAssistantAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using CookingAssistantAPI.Models;

namespace CookingAssistantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavouritesController : ControllerBase
    {
        private readonly CookingAssistantDbContext _context;
        private readonly ILogger<FavouritesController> _logger;

        public FavouritesController(CookingAssistantDbContext context, ILogger<FavouritesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/favourites/add - Add a recipe to favourites
        [HttpPost("add")]
        public async Task<IActionResult> AddFavourite([FromBody] FavouriteCreateModel favouriteModel)
        {
            _logger.LogInformation($"Adding recipe ID {favouriteModel.RecipeId} to favourites for user ID {favouriteModel.UserId}.");

            // Validate if the recipe exists
            var recipeExists = await _context.Recipes.AnyAsync(r => r.RecipeId == favouriteModel.RecipeId);
            if (!recipeExists)
            {
                _logger.LogWarning($"Recipe ID {favouriteModel.RecipeId} not found.");
                return BadRequest(new { Error = "Invalid RecipeId." });
            }

            // Validate if the user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == favouriteModel.UserId);
            if (!userExists)
            {
                _logger.LogWarning($"User ID {favouriteModel.UserId} not found.");
                return BadRequest(new { Error = "Invalid UserId." });
            }

            // Check if the favourite already exists
            var existingFavourite = await _context.Favourites
                .FirstOrDefaultAsync(f => f.RecipeId == favouriteModel.RecipeId && f.UserId == favouriteModel.UserId);

            if (existingFavourite != null)
            {
                _logger.LogWarning($"Recipe ID {favouriteModel.RecipeId} is already marked as favourite by user ID {favouriteModel.UserId}.");
                return Conflict(new { Message = "Recipe is already marked as favourite." });
            }

            // Add the favourite
            var newFavourite = new Favourite
            {
                RecipeId = favouriteModel.RecipeId,
                UserId = favouriteModel.UserId
            };

            _context.Favourites.Add(newFavourite);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Recipe ID {favouriteModel.RecipeId} successfully added to favourites for user ID {favouriteModel.UserId}.");
            return Ok(new { Message = "Recipe added to favourites." });
        }

        // DELETE: api/favourites/remove - Remove a recipe from favourites
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFavourite([FromBody] FavouriteCreateModel favouriteModel)
        {
            _logger.LogInformation($"Removing recipe ID {favouriteModel.RecipeId} from favourites for user ID {favouriteModel.UserId}.");

            // Find the favourite entry
            var favourite = await _context.Favourites
                .FirstOrDefaultAsync(f => f.RecipeId == favouriteModel.RecipeId && f.UserId == favouriteModel.UserId);

            if (favourite == null)
            {
                _logger.LogWarning($"Favourite entry not found for recipe ID {favouriteModel.RecipeId} and user ID {favouriteModel.UserId}.");
                return NotFound(new { Error = "Favourite entry not found." });
            }

            // Remove the favourite
            _context.Favourites.Remove(favourite);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Recipe ID {favouriteModel.RecipeId} successfully removed from favourites for user ID {favouriteModel.UserId}.");
            return Ok(new { Message = "Recipe removed from favourites." });
        }

        // GET: api/favourites/{userId} - Get all favourites for a specific user
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFavourites(int userId)
        {
            _logger.LogInformation($"Fetching favourites for user ID {userId}.");

            // Validate if the user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                _logger.LogWarning($"User ID {userId} not found.");
                return BadRequest(new { Error = "Invalid UserId." });
            }

            // Fetch favourites for the user
            var favourites = await _context.Favourites
                .Where(f => f.UserId == userId)
                .Include(f => f.Recipe) // Include recipe details
                .Select(f => new
                {
                    f.RecipeId,
                    RecipeTitle = f.Recipe.Title,
                    RecipeImageUrl = $"/api/recipes/{f.RecipeId}/image" // Generate image URL for frontend
                })
                .ToListAsync();

            if (!favourites.Any())
            {
                _logger.LogInformation($"No favourites found for user ID {userId}.");
                return NoContent();
            }

            _logger.LogInformation($"Retrieved {favourites.Count} favourites for user ID {userId}.");
            return Ok(favourites);
        }
    }
}