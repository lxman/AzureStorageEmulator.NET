namespace AzureStorageEmulator.NET.Blob.Models;

public enum LeaseState
{
    Available,
    Breaking,
    Broken,
    Expired,
    Leased,
    Unknown
}