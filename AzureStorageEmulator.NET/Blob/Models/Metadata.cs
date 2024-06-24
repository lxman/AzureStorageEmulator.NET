namespace AzureStorageEmulator.NET.Blob.Models
{
    public class Metadata
    {
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        public ulong Etag { get; set; } = (ulong)new Random().NextInt64(1000000000, 9999999999);

        public LeaseStatus LeaseStatus { get; set; } = LeaseStatus.Unlocked;

        public LeaseState LeaseState { get; set; } = LeaseState.Available;

        public bool HasImmutabilityPolicy { get; set; }

        public bool HasLegalHold { get; set; }
    }
}