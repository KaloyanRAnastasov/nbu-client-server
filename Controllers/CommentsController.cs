using Microsoft.AspNetCore.Mvc;
using KaviClientServerProject.Models;
using System.Threading.Tasks;
using System.Linq;

namespace KaviClientServerProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all comments
        [HttpGet]
        public IActionResult GetComments()
        {
            var comments = _context.Comments
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
            return Ok(comments);
        }

        // Add a new comment
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] Comment comment)
        {
            if (string.IsNullOrWhiteSpace(comment.Content))
            {
                return BadRequest("Comment content cannot be empty.");
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                // User is authenticated
                comment.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                comment.UserName = User.Identity.Name ?? "Unknown User"; // Email or username from Identity
            }
            else
            {
                // User is anonymous
                comment.UserId = null; // Anonymous user has no ID
                comment.UserName = string.IsNullOrWhiteSpace(comment.UserName) ? "Anonymous" : comment.UserName;
            }

            comment.CreatedAt = DateTime.UtcNow;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Return the comment and redirect URL
            var response = new
            {
                comment = new
                {
                    comment.UserName,
                    comment.Content,
                    comment.CreatedAt
                },
                redirect = User.Identity?.IsAuthenticated == true ? "/MyComments" : null
            };

            return Ok(response);
        }
    }
}
