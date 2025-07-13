using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters
{
    public class ApiKeyAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private const string API_KEY_HEADER = "X-Api-Key";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var logger = context.HttpContext.RequestServices
                .GetService(typeof(ILogger<ApiKeyAuthorizeAttribute>)) as ILogger<ApiKeyAuthorizeAttribute>;

            var httpContext = context.HttpContext;
            var request = httpContext.Request;

            var requestId = httpContext.TraceIdentifier;
            var route = context.ActionDescriptor.DisplayName;

            var configuredKey = Environment.GetEnvironmentVariable("API_KEY");

            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                logger?.LogWarning("RequestId={RequestId}, Route={Route} => API_KEY is not configured.", requestId, route);
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            if (!httpContext.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedKey))
            {
                logger?.LogWarning("RequestId={RequestId}, Route={Route} => Missing API key header.", requestId, route);
                context.Result = new UnauthorizedResult();
                return;
            }

            if (extractedKey != configuredKey)
            {
                logger?.LogWarning("RequestId={RequestId}, Route={Route} => Invalid API key provided.", requestId, route);
                context.Result = new UnauthorizedResult();
                return;
            }

            logger?.LogInformation("RequestId={RequestId}, Route={Route} => API key validated successfully.", requestId, route);
        }
    }
}
