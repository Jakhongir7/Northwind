using Microsoft.AspNetCore.Mvc.Filters;
using Northwind.Infrastructure.Attributes;

namespace Northwind.Infrastructure.Filters
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
            _logger.LogInformation("Starting action: {ActionName}", actionName);

            // Log parameters if enabled
            if (shouldLogParameters)
            {
                foreach (var (key, value) in context.ActionArguments)
                {
                    // Serialize complex objects to JSON for better readability
                    var serializedValue = value != null ? System.Text.Json.JsonSerializer.Serialize(value) : "null";
                    _logger.LogInformation("Parameter: {ParameterName} = {ParameterValue}", key, serializedValue);
                }
            }

            // Proceed to the next action filter or action method
            await next();

            // Log the action end
            _logger.LogInformation("Finished action: {ActionName}", actionName);
        }
    }
}
