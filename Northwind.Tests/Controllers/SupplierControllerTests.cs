using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Northwind.Controllers;
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
    public class SupplierControllerTests
    {
        private Mock<NorthwindContext> _mockContext;
        private Mock<DbSet<Supplier>> _mockSuppliersDbSet;
        private SupplierController _controller;

        [SetUp]
        public void Setup()
        {
            // Create a mock DbSet of Supplier
            var suppliersData = new List<Supplier>
            {
                new Supplier { SupplierID = 1, CompanyName = "Supplier A" },
                new Supplier { SupplierID = 2, CompanyName = "Supplier B" }
            }.AsQueryable();

            _mockSuppliersDbSet = new Mock<DbSet<Supplier>>();

            // Setup the mock DbSet to support LINQ operations
            _mockSuppliersDbSet.As<IQueryable<Supplier>>().Setup(m => m.Provider).Returns(suppliersData.Provider);
            _mockSuppliersDbSet.As<IQueryable<Supplier>>().Setup(m => m.Expression).Returns(suppliersData.Expression);
            _mockSuppliersDbSet.As<IQueryable<Supplier>>().Setup(m => m.ElementType).Returns(suppliersData.ElementType);
            _mockSuppliersDbSet.As<IQueryable<Supplier>>().Setup(m => m.GetEnumerator()).Returns(suppliersData.GetEnumerator());

            // Create a mock context and set up the Suppliers DbSet
            _mockContext = new Mock<NorthwindContext>(new DbContextOptions<NorthwindContext>());
            _mockContext.Setup(c => c.Suppliers).Returns(_mockSuppliersDbSet.Object);

            // Initialize the controller with the mock context
            _controller = new SupplierController(_mockContext.Object);
        }

        [Test]
        public void Index_ReturnsViewResult_WithListOfSuppliers()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult)result;

            // Check the model type and data
            var model = viewResult.ViewData.Model as List<Supplier>;
            Assert.IsNotNull(model);
            Assert.That(model.Count, Is.EqualTo(2));
            Assert.That(model[0].CompanyName, Is.EqualTo("Supplier A"));
            Assert.That(model[1].CompanyName, Is.EqualTo("Supplier B"));
        }
    }

}
