using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Northwind.Controllers;
using Northwind.Data;
using Northwind.Models;

namespace Northwind.Tests.Controllers
{
    [TestFixture]
    public class CategoryControllerTests
    {
        private Mock<NorthwindContext> _mockContext;
        private Mock<DbSet<Category>> _mockCategoriesDbSet;
        private CategoryController _controller;

        [SetUp]
        public void Setup()
        {
            // Create a mock list of categories
            var categoriesData = new List<Category>
        {
            new Category { CategoryID = 1, CategoryName = "Beverages" },
            new Category { CategoryID = 2, CategoryName = "Condiments" }
        }.AsQueryable();

            // Mock the DbSet<Category> to support LINQ operations
            _mockCategoriesDbSet = new Mock<DbSet<Category>>();
            _mockCategoriesDbSet.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(categoriesData.Provider);
            _mockCategoriesDbSet.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(categoriesData.Expression);
            _mockCategoriesDbSet.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(categoriesData.ElementType);
            _mockCategoriesDbSet.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(categoriesData.GetEnumerator());

            // Create a mock context and set up the Categories DbSet
            _mockContext = new Mock<NorthwindContext>(new DbContextOptions<NorthwindContext>());
            _mockContext.Setup(c => c.Categories).Returns(_mockCategoriesDbSet.Object);

            // Initialize the controller with the mock context
            _controller = new CategoryController(_mockContext.Object);
        }

        [Test]
        public void Index_ReturnsViewResult_WithListOfCategories()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult)result;

            // Check the model type and data
            var model = viewResult.ViewData.Model as List<Category>;
            Assert.IsNotNull(model);
            Assert.That(model.Count, Is.EqualTo(2));
            Assert.That(model[0].CategoryName, Is.EqualTo("Beverages"));
            Assert.That(model[1].CategoryName, Is.EqualTo("Condiments"));
        }
    }
}
