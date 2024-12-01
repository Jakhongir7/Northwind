using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Northwind.Data;
using Northwind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northwind.Tests.Controllers
{
    [TestFixture]
    public class ProductControllerTests
    {
        private Mock<NorthwindContext> _mockContext;
        private Mock<DbSet<Product>> _mockProducts;
        private Mock<DbSet<Category>> _mockCategories;
        private Mock<DbSet<Supplier>> _mockSuppliers;
        private Mock<IOptions<ProductSettings>> _mockSettings;
        private ProductController _controller;
        private List<Product> _productData;
        private List<Category> _categoryData;
        private List<Supplier> _supplierData;

        [SetUp]
        public void Setup()
        {
            // Setup initial product data
            _productData = new List<Product>
            {
                new Product { ProductID = 1, ProductName = "Chai", SupplierID = 1, CategoryID = 1, Discontinued = false },
                new Product { ProductID = 2, ProductName = "Chang", SupplierID = 2, CategoryID = 1, Discontinued = false }
            }.AsQueryable().ToList();

            // Setup initial category data
            _categoryData = new List<Category>
            {
                new Category { CategoryID = 1, CategoryName = "Beverages" },
                new Category { CategoryID = 2, CategoryName = "Condiments" }
            }.AsQueryable().ToList();

            // Setup initial supplier data
            _supplierData = new List<Supplier>
            {
                new Supplier { SupplierID = 1, CompanyName = "Supplier A" },
                new Supplier { SupplierID = 2, CompanyName = "Supplier B" }
            }.AsQueryable().ToList();

            // Create a mock of the DbSet<Product>
            _mockProducts = new Mock<DbSet<Product>>();
            _mockProducts.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(_productData.AsQueryable().Provider);
            _mockProducts.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(_productData.AsQueryable().Expression);
            _mockProducts.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(_productData.AsQueryable().ElementType);
            _mockProducts.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(_productData.AsQueryable().GetEnumerator());

            // Create mock for DbSet<Category>
            _mockCategories = new Mock<DbSet<Category>>();
            _mockCategories.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(_categoryData.AsQueryable().Provider);
            _mockCategories.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(_categoryData.AsQueryable().Expression);
            _mockCategories.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(_categoryData.AsQueryable().ElementType);
            _mockCategories.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(_categoryData.AsQueryable().GetEnumerator());

            // Create mock for DbSet<Supplier>
            _mockSuppliers = new Mock<DbSet<Supplier>>();
            _mockSuppliers.As<IQueryable<Supplier>>().Setup(m => m.Provider).Returns(_supplierData.AsQueryable().Provider);
            _mockSuppliers.As<IQueryable<Supplier>>().Setup(m => m.Expression).Returns(_supplierData.AsQueryable().Expression);
            _mockSuppliers.As<IQueryable<Supplier>>().Setup(m => m.ElementType).Returns(_supplierData.AsQueryable().ElementType);
            _mockSuppliers.As<IQueryable<Supplier>>().Setup(m => m.GetEnumerator()).Returns(_supplierData.AsQueryable().GetEnumerator());

            // Setup mock for Add and Find methods
            _mockProducts.Setup(m => m.Add(It.IsAny<Product>())).Callback<Product>(p => _productData.Add(p));
            _mockProducts.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => _productData.FirstOrDefault(p => p.ProductID == (int)ids[0]));

            // Create a mock of the NorthwindContext
            _mockContext = new Mock<NorthwindContext>(new DbContextOptions<NorthwindContext>());
            _mockContext.Setup(c => c.Products).Returns(_mockProducts.Object);
            _mockContext.Setup(c => c.Categories).Returns(_mockCategories.Object);
            _mockContext.Setup(c => c.Suppliers).Returns(_mockSuppliers.Object);

            // Mock IOptions<ProductSettings>
            _mockSettings = new Mock<IOptions<ProductSettings>>();
            _mockSettings.Setup(s => s.Value).Returns(new ProductSettings { MaxProducts = 10 });

            // Initialize the controller with the mocked context and settings
            _controller = new ProductController(_mockContext.Object, _mockSettings.Object);
        }

        [Test]
        public void Index_ReturnsViewResult_WithListOfProducts()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult)result;
            var model = viewResult.ViewData.Model as List<Product>;
            Assert.That(model, Has.Count.EqualTo(2)); // Verify that the model contains 2 products
        }

        [Test]
        public void Create_Get_ReturnsViewResult_WithViewBagData()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.ViewData["Categories"]);
            Assert.IsNotNull(viewResult.ViewData["Suppliers"]);
        }

        [Test]
        public void Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var product = new Product { ProductID = 5, ProductName = "Tea", SupplierID = 1, CategoryID = 1, Discontinued = false };

            // Act
            var result = _controller.Create(product);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));

            // Verify that the product was added to the mock DbSet
            Assert.That(_productData, Has.Exactly(1).Matches<Product>(p => p.ProductID == 5 && p.ProductName == "Tea"));
        }

        [Test]
        public void Edit_Get_ExistingProduct_ReturnsViewResult()
        {
            // Act
            var result = _controller.Edit(1);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.That(viewResult.Model, Is.EqualTo(_productData.First(p => p.ProductID == 1)));
        }

        [Test]
        public void Edit_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var product = _productData.First(p => p.ProductID == 1);
            product.ProductName = "Chai Updated";

            // Act
            var result = _controller.Edit(product);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));

            // Verify that the product's name was updated in the mock DbSet
            var updatedProduct = _productData.First(p => p.ProductID == 1);
            Assert.IsNotNull(updatedProduct);
            Assert.That(updatedProduct.ProductName, Is.EqualTo("Chai Updated"));
        }
    }
}
