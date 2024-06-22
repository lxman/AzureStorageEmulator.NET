namespace AzureStorageEmulator.NET.Blob.Models
{
    public class Metadata
    {
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        public ulong Etag { get; set; } = (ulong)new Random().NextInt64(1000000000, 9999999999);

        public string LeaseStatus { get; set; } = "unlocked";

        public string LeaseState { get; set; } = "available";

        public bool HasImmutabilityPolicy { get; set; }

        public bool HasLegalHold { get; set; }
    }
}
