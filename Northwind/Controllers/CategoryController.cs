using Microsoft.AspNetCore.Mvc;
using Northwind.Data;

namespace Northwind.Controllers
{
    public class CategoryController : Controller
    {
        private readonly NorthwindContext _context;

        public CategoryController(NorthwindContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // New Action to return category image
        [Route("images/{id}")]
        public IActionResult GetCategoryImage(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryID == id);

            if (category == null || category.Picture == null)
            {
                return NotFound();
            }

            var mimeType = "image/jpeg"; // Default to JPEG
            var imageBytes = category.Picture;

            // If you know the images are BMP, skip the first 78 bytes
            if (category.Picture.Length > 78 && IsBmp(category.Picture.Skip(78).ToArray()))
            {
                imageBytes = category.Picture.Skip(78).ToArray();
                mimeType = "image/bmp";
            }

            return File(imageBytes, mimeType);
        }

        private bool IsBmp(byte[] bytes)
        {
            // Check BMP header (first two bytes are 'BM')
            return bytes.Length >= 2 && bytes[0] == 0x42 && bytes[1] == 0x4D;
        }

        // GET: Edit category image
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
    }
}
