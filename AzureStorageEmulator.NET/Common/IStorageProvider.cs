namespace AzureStorageEmulator.NET.Common
{
    public interface IStorageProvider
    {
        Task Persist(string location);

        Task Restore(string location);

        void Delete(string location);
    }
}