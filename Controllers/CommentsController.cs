using CookingAssistantAPI.Data;
using CookingAssistantAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CookingAssistantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly CookingAssistantDbContext _context;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(CookingAssistantDbContext context, ILogger<CommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/comments/add - Submit a comment for a recipe
        [HttpPost("add")]
        public async Task<IActionResult> SubmitComment([FromBody] CommentCreateModel commentModel)
        {
            _logger.LogInformation($"Submitting comment for Recipe ID {commentModel.RecipeId} by User ID {commentModel.UserId}.");

            // Validate the model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid comment model state.");
                return BadRequest(ModelState);
            }

            // Validate if the recipe exists
            var recipeExists = await _context.Recipes.AnyAsync(r => r.RecipeId == commentModel.RecipeId);
            if (!recipeExists)
            {
                _logger.LogWarning($"Invalid RecipeId: {commentModel.RecipeId}.");
                return BadRequest(new { Error = "Invalid RecipeId." });
            }

            // Validate if the user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == commentModel.UserId);
            if (!userExists)
            {
                _logger.LogWarning($"Invalid UserId: {commentModel.UserId}.");
                return BadRequest(new { Error = "Invalid UserId." });
            }

            // Create a new comment
            var newComment = new Comment
            {
                RecipeId = commentModel.RecipeId,
                UserId = commentModel.UserId,
                Content = commentModel.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Comment with ID {newComment.CommentId} created successfully for Recipe ID {commentModel.RecipeId} by User ID {commentModel.UserId}.");
            return Ok(new { Message = "Comment submitted successfully." });
        }

        // GET: api/comments/{recipeId} - Get all comments for a specific recipe
        [HttpGet("{recipeId}")]
        public async Task<IActionResult> GetCommentsForRecipe(int recipeId)
        {
            _logger.LogInformation($"Fetching comments for Recipe ID {recipeId}.");

            // Validate if the recipe exists
            var recipeExists = await _context.Recipes.AnyAsync(r => r.RecipeId == recipeId);
            if (!recipeExists)
            {
                _logger.LogWarning($"Recipe with ID {recipeId} not found.");
                return NotFound();  // Return 404 if recipe not found
            }

            // Fetch comments for the specified recipe
            var comments = await _context.Comments
                .Where(c => c.RecipeId == recipeId)
                .Include(c => c.User)  // Include user to show who made the comment
                .Select(c => new
                {
                    c.CommentId,
                    c.Content,
                    c.CreatedAt,
                    UserName = c.User.UserName,
                    UserId = c.UserId  // Include UserId to check ownership
                })
                .OrderByDescending(c => c.CreatedAt)  // Order by newest comments first
                .ToListAsync();

            if (comments == null || comments.Count == 0)
            {
                _logger.LogWarning($"No comments found for Recipe ID {recipeId}.");
                return NoContent();  // Return 204 if no comments exist for the recipe
            }

            _logger.LogInformation($"Retrieved {comments.Count} comments for Recipe ID {recipeId}.");
            return Ok(comments);
        }

        // DELETE: api/comments/{commentId} - Delete a specific comment
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            _logger.LogInformation($"Attempting to delete comment with ID {commentId}.");

            // Fetch the comment from the database
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
            if (comment == null)
            {
                _logger.LogWarning($"Comment with ID {commentId} not found.");
                return NotFound(new { Error = "Comment not found." });  // Return 404 if the comment doesn't exist
            }

            // Remove the comment
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Comment with ID {commentId} deleted successfully.");
            return Ok(new { Message = "Comment deleted successfully." });
        }
    }

    // CommentCreateModel to handle comment creation requests
    public class CommentCreateModel
    {
        public int UserId { get; set; }  // The ID of the user submitting the comment
        public int RecipeId { get; set; }  // The ID of the recipe the comment belongs to
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;  // The content of the comment
    }
}
