using AzureStorageEmulator.NET.Common.HeaderManagement;
using Microsoft.AspNetCore.Diagnostics;

namespace AzureStorageEmulator.NET.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly HeaderManagement _headerManagement = new();

        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            bool handled = false;
            switch (exception)
            {
                case UnauthorizedAccessException:
                    httpContext.Response.StatusCode = 403;
                    _headerManagement.SetResponseHeaders(httpContext);
                    handled = true;
                    break;
            }
            return new ValueTask<bool>(handled);
        }
    }
}
