using AzureStorageEmulator.NET.Authorization.Queue;
using AzureStorageEmulator.NET.Authorization.Table;

namespace AzureStorageEmulator.NET.Authorization
{
    public class Authorizer : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            int port = context.Connection.LocalPort;
            string authorization = context.Request.Headers.Authorization.First()
                                     ?? throw new UnauthorizedAccessException();
            switch (port)
            {
                case 10000:
                    // Blob storage
                    break;

                case 10001:
                    if (!new QueueAuthorizer().Invoke(authorization, context))
                    {
                        throw new UnauthorizedAccessException();
                    }
                    break;

                case 10002:
                    if (!new TableAuthorizer().Invoke(authorization, context))
                    {
                        throw new UnauthorizedAccessException();
                    }
                    break;
            }
            await next.Invoke(context);
        }
    }
}