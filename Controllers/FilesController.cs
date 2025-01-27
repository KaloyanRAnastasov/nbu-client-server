using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using KaviClientServerProject.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KaviClientServerProject.Controllers
{
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FilesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var files = _context.FileItems.ToList();
            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View(files);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile uploadedFile)
        {
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "files");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Path.GetFileNameWithoutExtension(uploadedFile.FileName)}_{Path.GetRandomFileName()}{Path.GetExtension(uploadedFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }

                var fileItem = new FileItem
                {
                    FileName = uploadedFile.FileName,
                    FilePath = $"/files/{uniqueFileName}"
                };

                _context.FileItems.Add(fileItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult DeleteFile(int id)
        {
            var fileItem = _context.FileItems.FirstOrDefault(f => f.Id == id);
            if (fileItem != null)
            {
                var filePath = Path.Combine(_environment.WebRootPath, fileItem.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.FileItems.Remove(fileItem);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult DownloadFile(int id)
        {
            var fileItem = _context.FileItems.FirstOrDefault(f => f.Id == id);
            if (fileItem != null)
            {
                var filePath = Path.Combine(_environment.WebRootPath, fileItem.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", fileItem.FileName);
                }
            }

            return NotFound();
        }
    }
}
