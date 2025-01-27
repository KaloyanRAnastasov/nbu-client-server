using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using KaviClientServerProject.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KaviClientServerProject.Controllers
{

    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public GalleryController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var galleryItems = _context.GalleryItems.ToList();
            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View(galleryItems);
        }


        [HttpPost]
        public async Task<IActionResult> Add(string title, IFormFile imageFile)
        {
            if (string.IsNullOrEmpty(title) || imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Both title and image are required.");
                return RedirectToAction(nameof(Index)); // Redirect back to the gallery page
            }

            // Save the uploaded file to the server
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Save the gallery item to the database
            var galleryItem = new GalleryItem
            {
                Title = title,
                ImagePath = $"/uploads/{uniqueFileName}" // Relative path for serving the image
            };

            _context.GalleryItems.Add(galleryItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Remove(int id)
        {
            var item = _context.GalleryItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                // Delete the image file from the server
                var filePath = Path.Combine(_environment.WebRootPath, item.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.GalleryItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
