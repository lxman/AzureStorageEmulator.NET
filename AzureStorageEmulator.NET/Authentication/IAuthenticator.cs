namespace AzureStorageEmulator.NET.Authentication
{
    public interface IAuthenticator<T> where T : class
    {
        bool Authenticate(HttpRequest headers);
    }
}