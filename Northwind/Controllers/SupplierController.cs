using Microsoft.AspNetCore.Mvc;
using Northwind.Data;

namespace Northwind.Controllers
{
    public class SupplierController : Controller
    {
        private readonly NorthwindContext _context;

        public SupplierController(NorthwindContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Suppliers.ToList();
            return View(categories);
        }
    }
}
