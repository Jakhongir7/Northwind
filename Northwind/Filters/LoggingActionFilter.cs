using Microsoft.AspNetCore.Mvc.Filters;
using Northwind.Models.Attributes;

namespace Northwind.Filters
{
    public class LoggingActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<LoggingActionFilter> _logger;
        private readonly bool _logParameters;

        public LoggingActionFilter(ILogger<LoggingActionFilter> logger, bool logParameters = false)
        {
            _logger = logger;
            _logParameters = logParameters;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var actionDescriptor = context.ActionDescriptor;

            // Check if the LogParametersAttribute is present and enabled
            var logParametersAttribute = actionDescriptor.EndpointMetadata
                .OfType<LogParametersAttribute>()
                .FirstOrDefault();

            var shouldLogParameters = logParametersAttribute?.Enabled ?? _logParameters;

            // Log the action start
            var actionName = actionDescriptor.DisplayName;
            _logger.LogInformation($"Starting action: {actionName}");

            // Log parameters if enabled
            if (shouldLogParameters)
            {
                var parameters = context.ActionArguments;
                _logger.LogInformation($"Parameters: {string.Join(", ", parameters)}");
            }

            // Proceed to the next action filter or action method
            var resultContext = await next();

            // Log the action end
            _logger.LogInformation($"Finished action: {actionName}");
        }

    }
}
