using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Northwind.Data;
using Northwind.Models;
using System.Linq;

public class ProductController : Controller
{
    private readonly NorthwindContext _context;
    private readonly int _maxProducts;

    public ProductController(NorthwindContext context, IOptions<ProductSettings> productSettings)
    {
        _context = context;
        _maxProducts = productSettings.Value.MaxProducts;
    }

    public IActionResult Index()
    {
        var products = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        // Apply max product limit if configured
        if (_maxProducts > 0)
        {
            products = products.Take(_maxProducts);
        }

        return View(products.ToList());
    }

    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(_context.Categories, "CategoryID", "CategoryName");
        ViewBag.Suppliers = new SelectList(_context.Suppliers, "SupplierID", "CompanyName");
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Categories = new SelectList(_context.Categories, "CategoryID", "CategoryName", product.CategoryID);
        ViewBag.Suppliers = new SelectList(_context.Suppliers, "SupplierID", "CompanyName", product.SupplierID);
        return View(product);
    }

    public IActionResult Edit(int id)
    {
        var product = _context.Products.Find(id);
        if (product == null)
        {
            return NotFound();
        }
        ViewBag.Categories = new SelectList(_context.Categories, "CategoryID", "CategoryName", product.CategoryID);
        ViewBag.Suppliers = new SelectList(_context.Suppliers, "SupplierID", "CompanyName", product.SupplierID);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Update(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Categories = new SelectList(_context.Categories, "CategoryID", "CategoryName", product.CategoryID);
        ViewBag.Suppliers = new SelectList(_context.Suppliers, "SupplierID", "CompanyName", product.SupplierID);
        return View(product);
    }
}
