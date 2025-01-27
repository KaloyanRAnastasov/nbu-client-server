using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KaviClientServerProject.Models;
using System.Linq;

namespace KaviClientServerProject.Controllers
{
    [Authorize]
    public class MyCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MyCommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Retrieve the current user's ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Query the comments for the current user
            var userComments = _context.Comments
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return View(userComments);
        }
    }
}
