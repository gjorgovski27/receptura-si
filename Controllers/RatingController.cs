using CookingAssistantAPI.Data;
using CookingAssistantAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;

namespace CookingAssistantAPI.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly CookingAssistantDbContext _context;
    private readonly ILogger<RatingsController> _logger;

    public RatingsController(CookingAssistantDbContext context, ILogger<RatingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // POST: api/ratings/add - Submit a rating for a recipe
    [HttpPost("add")]
    public async Task<IActionResult> SubmitRating([FromBody] RatingCreateModel ratingModel)
    {
        _logger.LogInformation($"SubmitRating called with RecipeId: {ratingModel?.RecipeId}, UserId: {ratingModel?.UserId}, Value: {ratingModel?.Value}");

        if (ratingModel == null)
        {
            _logger.LogWarning("Received null RatingCreateModel.");
            return BadRequest("Invalid rating data.");
        }

        // Validate the model state
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid rating model state.");
            return BadRequest(ModelState);
        }

        try
        {
            // Validate if the recipe exists
            var recipeExists = await _context.Recipes.AnyAsync(r => r.RecipeId == ratingModel.RecipeId);
            if (!recipeExists)
            {
                _logger.LogWarning($"Recipe with ID: {ratingModel.RecipeId} does not exist.");
                return BadRequest(new { Error = "Invalid RecipeId." });
            }

            // Validate if the user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == ratingModel.UserId);
            if (!userExists)
            {
                _logger.LogWarning($"User with ID: {ratingModel.UserId} does not exist.");
                return BadRequest(new { Error = "Invalid UserId." });
            }

            // Check if a rating from this user for this recipe already exists
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.RecipeId == ratingModel.RecipeId && r.UserId == ratingModel.UserId);

            if (existingRating != null)
            {
                // Update the existing rating
                existingRating.Value = ratingModel.Value;
                _context.Ratings.Update(existingRating);
                _logger.LogInformation($"Updated rating for Recipe ID: {ratingModel.RecipeId} by User ID: {ratingModel.UserId}.");
            }
            else
            {
                // Create a new rating
                var newRating = new Rating
                {
                    RecipeId = ratingModel.RecipeId,
                    UserId = ratingModel.UserId,
                    Value = ratingModel.Value
                };
                _context.Ratings.Add(newRating);
                _logger.LogInformation($"Created a new rating for Recipe ID: {ratingModel.RecipeId} by User ID: {ratingModel.UserId}.");
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Rating submitted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while submitting rating: {ex.Message}");
            return StatusCode(500, "An error occurred while submitting the rating.");
        }
    }
        // GET: api/ratings/{recipeId}/average - Get the average rating for a specific recipe
        [HttpGet("{recipeId}/average")]
        public async Task<IActionResult> GetAverageRating(int recipeId)
        {
            _logger.LogInformation($"Fetching average rating for Recipe ID: {recipeId}.");

            // Validate if the recipe exists
            var recipe = await _context.Recipes
                .Include(r => r.Ratings)
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId);

            if (recipe == null)
            {
                _logger.LogWarning($"Recipe with ID {recipeId} not found.");
                return NotFound();  // Return 404 if recipe not found
            }

            // Calculate the average rating
            var averageRating = recipe.Ratings.Count > 0 ? recipe.Ratings.Average(r => r.Value) : 0.0;

            _logger.LogInformation($"Average rating for Recipe ID {recipeId} is {averageRating}.");
            return Ok(new { RecipeId = recipeId, AverageRating = averageRating });
        }
    }
}
