using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Northwind.Controllers;
using Northwind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northwind.Tests.Controllers
{
    [TestFixture]
    public class ErrorControllerTests
    {
        private Mock<ILogger<ErrorController>> _mockLogger;
        private ErrorController _controller;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ErrorController>>();
            _controller = new ErrorController(_mockLogger.Object);

            // Mock the HttpContext and the IExceptionHandlerFeature
            var httpContext = new DefaultHttpContext();
            var exceptionFeature = new ExceptionHandlerFeature
            {
                Error = new Exception("Test exception")
            };

            httpContext.Features.Set<IExceptionHandlerFeature>(exceptionFeature);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Test]
        public void Error_ReturnsViewResult_WithErrorViewModel()
        {
            // Act
            var result = _controller.Error();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.IsInstanceOf<ErrorViewModel>(viewResult.Model);
            var model = viewResult.Model as ErrorViewModel;
            Assert.That(model.ErrorMessage, Is.EqualTo("Test exception")); // Verify the error message is set correctly
        }
    }
}
