using CookingAssistantAPI.Data;
using CookingAssistantAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Threading.Tasks;

namespace CookingAssistantAPI.Pages
{
    public class CreateRecipeModel : PageModel
    {
        private readonly CookingAssistantDbContext _context;

        public CreateRecipeModel(CookingAssistantDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public RecipeCreateModel RecipeCreateModel { get; set; } = new RecipeCreateModel();

        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }

        public void OnGet(int userId)
        {
            UserId = userId;  // Set the UserId when loading the page
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // Return back to the form with validation errors
            }

            // Create a new Recipe entity and map the values
            var newRecipe = new Recipe
            {
                Title = RecipeCreateModel.Title,
                Description = RecipeCreateModel.Description,
                Ingredients = RecipeCreateModel.Ingredients,
                Instructions = RecipeCreateModel.Instructions,
                CookingTime = RecipeCreateModel.CookingTime,
                ServingSize = RecipeCreateModel.ServingSize,
                CreatedByUserId = UserId
            };

            // Handle image upload
            if (RecipeCreateModel.ImageFile != null)
            {
                if (RecipeCreateModel.ImageFile.Length > 5 * 1024 * 1024) // Limit size to 5 MB
                {
                    ModelState.AddModelError("ImageFile", "Image size cannot exceed 5 MB.");
                    return Page();
                }

                using (var dataStream = new MemoryStream())
                {
                    await RecipeCreateModel.ImageFile.CopyToAsync(dataStream);
                    newRecipe.ImageData = dataStream.ToArray(); // Store the image as a byte array
                }
            }

            try
            {
                _context.Recipes.Add(newRecipe);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the recipe. Please try again.");
                return Page();
            }

            // Redirect to the home page with userId after recipe is added
            return RedirectToPage("/HomePage", new { userId = UserId });
        }
    }
}
