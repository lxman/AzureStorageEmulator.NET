namespace AzureStorageEmulator.NET.Authentication
{
    public interface IAuthenticator
    {
        bool Authenticate(HttpRequest headers);
    }
}