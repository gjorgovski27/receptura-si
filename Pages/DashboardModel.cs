using CookingAssistantAPI.Data;
using CookingAssistantAPI.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CookingAssistantAPI.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly CookingAssistantDbContext _context;

        public DashboardModel(CookingAssistantDbContext context)
        {
            _context = context;
        }

        public List<News> NewsArticles { get; set; } = new List<News>();

        public async Task OnGetAsync()
        {
            // Fetch all news articles
            NewsArticles = await _context.News
                .OrderByDescending(n => n.CreatedAt) // Order by the most recent first
                .ToListAsync();
        }
    }
}
