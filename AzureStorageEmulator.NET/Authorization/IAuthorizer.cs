namespace AzureStorageEmulator.NET.Authorization
{
    public interface IAuthorizer<T> where T : class
    {
        bool Authorize(HttpRequest headers);
    }
}