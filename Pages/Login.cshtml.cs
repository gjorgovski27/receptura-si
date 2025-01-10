using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;

namespace CookingAssistantAPI.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string? Message { get; set; }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSubmit()
        {
            // Hardcoded admin credentials for Dashboard access
            const string adminEmail = "mihail@gmail.com";
            const string adminPassword = "olympuszeus";

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                Message = "Email and password are required.";
                return Page();
            }

            if (Email?.Trim() == adminEmail && Password?.Trim() == adminPassword)
            {
                return RedirectToPage("/Dashboard");
            }


            using var httpClient = new HttpClient();
            var loginRequest = new { Email, Password };
            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://recepturasi.azurewebsites.net/api/users/login", content);

            if (response.IsSuccessStatusCode)
            {
                Message = "Login successful!";

                // Extracting the userId from the response
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent);

                if (loginResponse != null && loginResponse.UserData != null)
                {
                    int userId = loginResponse.UserData.UserId;

                    // Redirect to HomePage with userId
                    return RedirectToPage("/HomePage", new { userId = userId });
                }
                else
                {
                    Message = "Error extracting user information from response.";
                }
            }
            else
            {
                Message = "Error: " + await response.Content.ReadAsStringAsync();
            }

            return Page();
        }

        // Helper class for deserializing the response
        public class LoginResponseDto
        {
            public string? Token { get; set; }
            public UserDto? UserData { get; set; }

            public class UserDto
            {
                public int UserId { get; set; }
                public string? UserName { get; set; }
                public string? UserFullName { get; set; }
                public string? UserEmail { get; set; }
            }
        }
    }
}
