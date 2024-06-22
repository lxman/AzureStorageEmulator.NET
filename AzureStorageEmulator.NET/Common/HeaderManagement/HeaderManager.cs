namespace AzureStorageEmulator.NET.Common.HeaderManagement
{
    public class HeaderManager : IMiddleware
    {
        private readonly HeaderManagement _headerManagement = new();

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _headerManagement.SetResponseHeaders(context);
            await next.Invoke(context);
        }
    }
}