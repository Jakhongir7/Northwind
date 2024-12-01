using Microsoft.AspNetCore.Mvc;
using Northwind.Data;

namespace Northwind.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly NorthwindContext _context;

        public CategoriesController(NorthwindContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllCategories()
        {
            var categories = _context.Categories.ToList();
            return Ok(categories);
        }

        [HttpPut("{id}/image")]
        public IActionResult UpdateCategoryImage(int id, [FromBody] byte[] picture)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();

            category.Picture = picture;
            _context.SaveChanges();
            return NoContent();
        }
    }
}
