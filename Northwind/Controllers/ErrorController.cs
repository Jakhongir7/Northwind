using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Northwind.Models;
using Northwind.Models.Attributes;
using Serilog;
using System.Diagnostics;

namespace Northwind.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error")]
        [LogParameters(true)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var error = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

            // Log the error details
            _logger.LogError(error, "An unhandled exception occurred");


            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = error?.Message
            });
        }
    }
}
