using Microsoft.AspNetCore.Mvc;
using Northwind.Data;
using Northwind.Infrastructure.Attributes;

namespace Northwind.Controllers
{
    public class SupplierController : Controller
    {
        private readonly NorthwindContext _context;

        public SupplierController(NorthwindContext context)
        {
            _context = context;
        }

        [LogParameters(true)]
        public IActionResult Index()
        {
            var categories = _context.Suppliers.ToList();
            return View(categories);
        }
    }
}
