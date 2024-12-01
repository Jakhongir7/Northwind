using Microsoft.AspNetCore.Mvc;
using Northwind.Data;
using Northwind.Infrastructure.Attributes;

namespace Northwind.Controllers
{
    public class CategoryController : Controller
    {
        private readonly NorthwindContext _context;

        public CategoryController(NorthwindContext context)
        {
            _context = context;
        }

        [LogParameters(true)]
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // New Action to return category image
        [Route("images/{id}")]
        [LogParameters(true)]
        [HttpGet]
        public IActionResult GetCategoryImage(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryID == id);

            if (category == null || category.Picture == null)
            {
                return NotFound();
            }

            byte[] imageBytes = category.Picture;
            string mimeType = "image/jpeg"; // Default to JPEG

            // If you know the images are BMP, skip the first 78 bytes
            if (category.Picture.Length > 78 && IsBmp(category.Picture.Skip(78).ToArray()))
            {
                imageBytes = category.Picture.Skip(78).ToArray();
                mimeType = "image/bmp";
            }

            return File(imageBytes, mimeType);
        }

        // GET: Edit category image
        [LogParameters(true)]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Update category image
        [HttpPost]
        [LogParameters(true)]
        public IActionResult Edit(int id, IFormFile newImage)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound();
            }

            if (newImage != null && newImage.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    newImage.CopyTo(memoryStream);
                    // Save image to category
                    category.Picture = memoryStream.ToArray();
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        private static bool IsBmp(byte[] bytes)
        {
            // Check BMP header (first two bytes are 'BM')
            return bytes.Length >= 2 && bytes[0] == 0x42 && bytes[1] == 0x4D;
        }
    }
}
