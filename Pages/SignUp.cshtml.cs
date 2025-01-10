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
public string UserName { get; set; }

[BindProperty]
public string UserFullName { get; set; }

[BindProperty]
public string UserEmail { get; set; }

[BindProperty]
public string UserPassword { get; set; }

[BindProperty]
public string UserPhone { get; set; }


        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSubmit()
{
    var user = new
    {
        UserName = UserName,
        UserFullName = UserFullName,
        UserEmail = UserEmail,
        UserPassword = UserPassword,
        UserPhone = UserPhone
    };

    using var httpClient = new HttpClient();
    var json = JsonSerializer.Serialize(user);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await httpClient.PostAsync("https://recepturasi.azurewebsites.net/api/users/signup", content);

    if (response.IsSuccessStatusCode)
    {
        Message = "Sign-up successful!";
        return RedirectToPage("/Login");
    }
    else
    {
        Message = "Error: " + await response.Content.ReadAsStringAsync();
    }

    return Page();
}

    }
}
