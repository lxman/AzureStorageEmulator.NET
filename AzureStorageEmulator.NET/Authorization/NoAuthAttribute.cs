namespace AzureStorageEmulator.NET.Authorization
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NoAuthAttribute : Attribute
    {
    }
}
