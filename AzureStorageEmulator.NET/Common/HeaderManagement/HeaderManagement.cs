using Microsoft.Extensions.Primitives;

namespace AzureStorageEmulator.NET.Common.HeaderManagement
{
    public class HeaderManagement : IHeaderManagement
    {
        public void SetResponseHeaders(HttpContext context)
        {
            context.Response.Headers.Append("x-ms-version", "2023-11-03");
            context.Response.Headers.Append("x-ms-request-id", Guid.NewGuid().ToString());
            if (context.Request.Headers.TryGetValue("x-ms-client-request-id", out StringValues clientRequestId) && clientRequestId.Count > 0)
            {
                context.Response.Headers.Append("x-ms-client-request-id", clientRequestId);
            }
        }
    }
}