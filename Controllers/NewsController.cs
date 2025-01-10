using CookingAssistantAPI.Data;
using CookingAssistantAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CookingAssistantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly CookingAssistantDbContext _context;
        private readonly ILogger<NewsController> _logger;

        public NewsController(CookingAssistantDbContext context, ILogger<NewsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/news/create - Create a news article
        [HttpPost("create")]
        public async Task<IActionResult> CreateNews([FromForm] NewsCreateModel newsModel)
        {
            _logger.LogInformation("Creating a new news article.");

            // Validate the model
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid news model.");
                return BadRequest(ModelState);
            }

            try
            {
                // Convert the IFormFile to a byte array
                byte[] imageBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await newsModel.ArticleImage.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }

                // Create a new News entity
                var newsArticle = new News
                {
                    ArticleDescription = newsModel.ArticleDescription,
                    ArticleContent = newsModel.ArticleContent,
                    ArticleImage = imageBytes,
                    ImageMimeType = newsModel.ArticleImage.ContentType, // Get MIME type
                    CreatedAt = newsModel.CreatedAt != default ? newsModel.CreatedAt : DateTime.UtcNow // Set timestamp if not provided
                };

                // Add and save the news article
                _context.News.Add(newsArticle);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"News article with ID {newsArticle.ArticleId} created successfully.");
                return Ok(new { Message = "News article created successfully.", ArticleId = newsArticle.ArticleId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the news article.");
                return StatusCode(500, new { Error = "An error occurred while creating the news article." });
            }
        }

        // DELETE: api/news/delete/{articleId} - Delete a news article
        [HttpDelete("delete/{articleId}")]
        public async Task<IActionResult> DeleteNews(int articleId)
        {
            _logger.LogInformation($"Attempting to delete news article with ID {articleId}.");

            // Find the news article
            var newsArticle = await _context.News.FirstOrDefaultAsync(n => n.ArticleId == articleId);
            if (newsArticle == null)
            {
                _logger.LogWarning($"News article with ID {articleId} not found.");
                return NotFound(new { Error = "News article not found." });
            }

            _logger.LogInformation($"Deleting news article: {newsArticle.ArticleDescription}");

            // Delete the news article
            _context.News.Remove(newsArticle);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"News article with ID {articleId} deleted successfully.");
            return Ok(new { Message = "News article deleted successfully." });
        }

        // GET: api/news/all - Fetch all news articles
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNews()
        {
            _logger.LogInformation("Fetching all news articles.");

            var newsArticles = await _context.News
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.ArticleId,
                    n.ArticleDescription,
                    n.ArticleContent,
                    n.ImageMimeType,
                    n.CreatedAt
                })
                .ToListAsync();

            if (newsArticles.Count == 0)
            {
                _logger.LogWarning("No news articles found.");
                return NoContent(); // Return 204 if no news articles are found
            }

            _logger.LogInformation($"Retrieved {newsArticles.Count} news articles.");
            return Ok(newsArticles);
        }

        // GET: api/news/{id} - Fetch a single news article by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(int id)
        {
            _logger.LogInformation($"Fetching news article with ID {id}.");

            var newsArticle = await _context.News
                .Where(n => n.ArticleId == id)
                .Select(n => new
                {
                    n.ArticleId,
                    n.ArticleDescription,
                    n.ArticleContent,
                    n.ImageMimeType,
                    n.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (newsArticle == null)
            {
                _logger.LogWarning($"News article with ID {id} not found.");
                return NotFound(new { Error = "News article not found." });
            }

            _logger.LogInformation($"Retrieved news article with ID {id}.");
            return Ok(newsArticle);
        }

        // GET: api/news/{id}/image - Fetch the image of a news article by ID
        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetNewsImage(int id)
        {
            _logger.LogInformation($"Fetching image for news article with ID {id}.");

            var newsArticle = await _context.News.FirstOrDefaultAsync(n => n.ArticleId == id);

            if (newsArticle == null || newsArticle.ArticleImage == null || string.IsNullOrEmpty(newsArticle.ImageMimeType))
            {
                _logger.LogWarning($"Image for news article with ID {id} not found.");
                return NotFound(new { Error = "Image not found." });
            }

            _logger.LogInformation($"Returning image for news article with ID {id}.");
            return File(newsArticle.ArticleImage, newsArticle.ImageMimeType); // Return the image data as a file
        }
    }
}
