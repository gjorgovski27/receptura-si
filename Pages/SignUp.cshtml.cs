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

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSubmit()
        {
            var user = new
            {
                UserName = Request.Form["UserName"],
                UserFullName = Request.Form["FullName"],
                UserEmail = Request.Form["Email"],
                UserPassword = Request.Form["Password"],
                UserPhone = Request.Form["Phone"]
            };

            using var httpClient = new HttpClient();
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://localhost:5203/api/users/signup", content);

            if (response.IsSuccessStatusCode)
            {
                Message = "Sign-up successful!";
            }
            else
            {
                Message = "Error: " + await response.Content.ReadAsStringAsync();
            }

            return Page();
        }
    }
}
