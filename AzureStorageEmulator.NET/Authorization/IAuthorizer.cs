namespace AzureStorageEmulator.NET.Authorization
{
    public interface IAuthorizer
    {
        bool Authorize(HttpRequest request);
    }
}
