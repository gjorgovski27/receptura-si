using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CookingAssistantAPI.Pages
{
    public class SignUpModel : PageModel
    {
        [BindProperty]
        public string? Message { get; set; }

        [BindProperty]
        public string UserName { get; set; } = null!;

        [BindProperty]
        public string UserFullName { get; set; } = null!;

        [BindProperty]
        public string UserEmail { get; set; } = null!;

        [BindProperty]
        public string UserPassword { get; set; } = null!;

        [BindProperty]
        public string UserPhone { get; set; } = null!;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSubmitAsync()
        {
            // Check if the form data is valid
            if (!ModelState.IsValid)
            {
                Message = "Please ensure all fields are filled out correctly.";
                return Page();
            }

            // Create the user object to send to the API
            var user = new
            {
                UserName = UserName,
                UserFullName = UserFullName,
                UserEmail = UserEmail,
                UserPassword = UserPassword,
                UserPhone = UserPhone
            };

            // Serialize the user object to JSON
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Replace with the appropriate API URL
                var apiUrl = "https://recepturasi.azurewebsites.net/api/users/signup";
                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(apiUrl, content);

                // Handle the response
                if (response.IsSuccessStatusCode)
                {
                    Message = "Sign-up successful!";
                    return RedirectToPage("/Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Error: {errorContent}";
                    ModelState.AddModelError(string.Empty, $"Sign-up failed: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Log exception details and set error message
                Console.WriteLine($"Exception: {ex.Message}");
                Message = $"Error: {ex.Message}";
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            }

            // Return to the same page if the sign-up fails
            return Page();
        }
    }
}
