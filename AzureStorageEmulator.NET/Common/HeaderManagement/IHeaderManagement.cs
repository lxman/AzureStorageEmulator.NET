namespace AzureStorageEmulator.NET.Common.HeaderManagement
{
    public interface IHeaderManagement
    {
        void SetResponseHeaders(HttpContext context);
    }
}