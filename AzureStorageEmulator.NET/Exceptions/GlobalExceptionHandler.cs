using AzureStorageEmulator.NET.Common.HeaderManagement;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AzureStorageEmulator.NET.Exceptions
{
    public class GlobalExceptionHandler : ExceptionFilterAttribute, IExceptionHandler
    {
        private readonly HeaderManagement _headerManagement = new();

        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            bool handled = false;
            switch (exception)
            {
                case UnauthorizedAccessException:
                    httpContext.Response.StatusCode = 403;
                    handled = true;
                    break;
            }
            _headerManagement.SetResponseHeaders(httpContext);
            return new ValueTask<bool>(handled);
        }
    }
}