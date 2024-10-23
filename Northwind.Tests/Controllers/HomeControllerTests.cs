using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Northwind.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northwind.Tests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<ILogger<HomeController>> _mockLogger;
        private HomeController _controller;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_mockLogger.Object);
        }

        [Test]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }
    }
}
