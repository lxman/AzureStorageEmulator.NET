using AzureStorageEmulator.NET.Authorization.Queue;

namespace AzureStorageEmulator.NET.Authorization
{
    public class Authorizer : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            int port = context.Connection.LocalPort;
            string authentication = context.Request.Headers.Authorization.First()
                                     ?? throw new UnauthorizedAccessException();
            switch (port)
            {
                case 10000:
                    if (!new QueueAuthorizer().Invoke(authentication, context, next))
                    {
                        throw new UnauthorizedAccessException();
                    }
                    break;
                case 10001:
                    //await new TableAuthenticator().Invoke(context, next);
                    break;
                case 10002:
                    //await new BlobAuthenticator().Invoke(context, next);
                    break;
            }
            await next.Invoke(context);
        }
    }
}