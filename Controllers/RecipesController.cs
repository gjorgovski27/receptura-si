using CookingAssistantAPI.Data;
using CookingAssistantAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CookingAssistantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly CookingAssistantDbContext _context;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(CookingAssistantDbContext context, ILogger<RecipesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all recipes (including average rating)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllRecipes()
        {
            _logger.LogInformation("Fetching all recipes.");

            var recipes = await _context.Recipes
                .Include(r => r.CreatedByUser)
                .Select(r => new
                {
                    r.RecipeId,
                    r.Title,
                    r.Description,
                    r.CookingTime,
                    r.ServingSize,
                    AverageRating = _context.Ratings
                        .Where(rt => rt.RecipeId == r.RecipeId)
                        .Average(rt => (double?)rt.Value) ?? 0.0  // Calculate average rating or return 0 if no ratings
                })
                .ToListAsync();

            if (recipes == null || recipes.Count == 0)
            {
                _logger.LogWarning("No recipes found.");
                return NoContent();  // Return 204 if no recipes exist
            }

            _logger.LogInformation($"Retrieved {recipes.Count} recipes.");
            return Ok(recipes);
        }

        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetRecipeImage(int id)
        {
            _logger.LogInformation($"Fetching image for recipe with ID {id}.");

            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.RecipeId == id);

            if (recipe == null || recipe.ImageData == null || string.IsNullOrEmpty(recipe.ImageMimeType))
            {
                _logger.LogWarning($"Image for recipe with ID {id} not found.");
                return NotFound();  // Return 404 if image not found or doesn't exist
            }

            _logger.LogInformation($"Returning image for recipe with ID {id}.");
            return File(recipe.ImageData, recipe.ImageMimeType); // Return the image data as a file
        }


        // Get a recipe by ID (including average rating)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeById(int id)
        {
            _logger.LogInformation($"Fetching recipe with ID {id}.");

            var recipe = await _context.Recipes
                .Include(r => r.CreatedByUser)
                .Where(r => r.RecipeId == id)
                .Select(r => new
                {
                    r.RecipeId,
                    r.Title,
                    r.Description,
                    r.Ingredients,
                    r.Instructions,
                    r.CookingTime,
                    r.ServingSize,
                    AverageRating = _context.Ratings
                        .Where(rt => rt.RecipeId == r.RecipeId)
                        .Average(rt => (double?)rt.Value) ?? 0.0  // Calculate average rating or return 0 if no ratings
                })
                .FirstOrDefaultAsync();

            if (recipe == null)
            {
                _logger.LogWarning($"Recipe with ID {id} not found.");
                return NotFound();  // Return 404 if recipe not found
            }

            _logger.LogInformation($"Retrieved recipe with ID {id}.");
            return Ok(recipe);
        }

        // Create a new recipe
        [HttpPost("add")]
        public async Task<IActionResult> CreateRecipe([FromForm] RecipeCreateModel recipeModel)
        {
            _logger.LogInformation("Creating new recipe.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state.");
                return BadRequest(ModelState);
            }

            var userExists = await _context.Users.AnyAsync(u => u.UserId == recipeModel.CreatedByUserId);
            if (!userExists)
            {
                _logger.LogWarning($"Invalid CreatedByUserId: {recipeModel.CreatedByUserId}.");
                return BadRequest(new { Error = "Invalid CreatedByUserId." });
            }

            var newRecipe = new Recipe
            {
                Title = recipeModel.Title,
                Description = recipeModel.Description,
                Ingredients = recipeModel.Ingredients,
                Instructions = recipeModel.Instructions,
                CookingTime = recipeModel.CookingTime,
                ServingSize = recipeModel.ServingSize,
                CreatedByUserId = recipeModel.CreatedByUserId
            };

            if (recipeModel.ImageFile != null)
            {
                using var memoryStream = new MemoryStream();
                await recipeModel.ImageFile.CopyToAsync(memoryStream);
                newRecipe.ImageData = memoryStream.ToArray();
                newRecipe.ImageMimeType = recipeModel.ImageFile.ContentType;
            }

            _context.Recipes.Add(newRecipe);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Recipe with ID {newRecipe.RecipeId} created successfully.");
            return CreatedAtAction(nameof(GetRecipeById), new { id = newRecipe.RecipeId }, newRecipe);
        }

        // Rate a recipe
        [HttpPost("{id}/rate")]
        public async Task<IActionResult> RateRecipe(int id, [FromBody] RatingCreateModel ratingModel)
        {
            _logger.LogInformation($"Rating recipe with ID {id}.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid rating model state.");
                return BadRequest(ModelState);
            }

            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                _logger.LogWarning($"Recipe with ID {id} not found.");
                return NotFound();  // Return 404 if the recipe doesn't exist
            }

            var userExists = await _context.Users.AnyAsync(u => u.UserId == ratingModel.UserId);
            if (!userExists)
            {
                _logger.LogWarning($"Invalid UserId: {ratingModel.UserId}.");
                return BadRequest(new { Error = "Invalid UserId." });  // Return 400 if the user doesn't exist
            }

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.RecipeId == id && r.UserId == ratingModel.UserId);

            if (existingRating != null)
            {
                existingRating.Value = ratingModel.Value;  // Update the existing rating
                _context.Ratings.Update(existingRating);
                _logger.LogInformation($"Updated existing rating for Recipe ID {id} by User ID {ratingModel.UserId}.");
            }
            else
            {
                var newRating = new Rating
                {
                    RecipeId = id,
                    UserId = ratingModel.UserId,
                    Value = ratingModel.Value
                };
                _context.Ratings.Add(newRating);
                _logger.LogInformation($"Created new rating for Recipe ID {id} by User ID {ratingModel.UserId}.");
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Rating submitted successfully." });
        }

        [HttpGet("top-rated")]
public async Task<IActionResult> GetTopRatedRecipes()
{
    _logger.LogInformation("Fetching top-rated recipes.");

    var recipes = await _context.Recipes
        .Include(r => r.Ratings)
        .Where(r => r.Ratings.Average(rt => (double?)rt.Value) >= 4)
        .Select(r => new
        {
            r.RecipeId,
            r.Title,
            r.Description,
            r.CookingTime,
            r.ServingSize,
            AverageRating = _context.Ratings
                .Where(rt => rt.RecipeId == r.RecipeId)
                .Average(rt => (double?)rt.Value) ?? 0.0
        })
        .ToListAsync();

    if (recipes == null || recipes.Count == 0)
    {
        _logger.LogWarning("No top-rated recipes found.");
        return NoContent(); // Return 204 if no top-rated recipes exist
    }

    _logger.LogInformation($"Retrieved {recipes.Count} top-rated recipes.");
    return Ok(recipes);
}


        // Delete a recipe
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            _logger.LogInformation($"Deleting recipe with ID {id}.");

            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                _logger.LogWarning($"Recipe with ID {id} not found.");
                return NotFound();
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Recipe with ID {id} deleted successfully.");
            return NoContent();
        }


            // GET: api/recipes/search?query=pasta
[HttpGet("search")]
public async Task<IActionResult> SearchRecipes([FromQuery] string query)
{
    _logger.LogInformation($"Searching recipes with query: {query}");

    if (string.IsNullOrWhiteSpace(query))
    {
        _logger.LogWarning("Empty search query.");
        return NoContent(); // 204 if empty query
    }

    var results = await _context.Recipes
        .Where(r => r.Title.Contains(query) || (r.Description != null && r.Description.Contains(query)))
        .Select(r => new
        {
            r.RecipeId,
            r.Title,
            r.Description
        })
        .ToListAsync();

    if (results.Count == 0)
    {
        _logger.LogInformation("No matching recipes found.");
        return NoContent(); // 204 if no results
    }

    _logger.LogInformation($"Found {results.Count} recipes matching query.");
    return Ok(results);
}
    }

}
