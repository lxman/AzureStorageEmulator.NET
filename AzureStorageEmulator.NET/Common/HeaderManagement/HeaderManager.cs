namespace AzureStorageEmulator.NET.Common.HeaderManagement
{
    public class HeaderManager : IMiddleware
    {
        private readonly HeaderManagement _headerManagement = new();

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _headerManagement.SetResponseHeaders(context);
            next.Invoke(context);
            return Task.CompletedTask;
        }
    }
}
