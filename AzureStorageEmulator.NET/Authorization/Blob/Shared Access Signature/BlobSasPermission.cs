using System.Collections.ObjectModel;

namespace AzureStorageEmulator.NET.Authorization.Blob.Shared_Access_Signature
{
    public class BlobSasPermission
    {
        public static ReadOnlyDictionary<string, string> BlobSasPermissions = new(new Dictionary<string, string>
        {
            { "r", "Read" },
            { "a", "Add" },
            { "c", "Create" },
            { "w", "Write" },
            { "d", "Delete" },
            { "x", "DeleteVersion" },
            { "t", "Tag" },
            { "m", "Move" },
            { "e", "Execute" },
            { "i", "SetImmutabilityPolicy" },
            { "y", "PermanentDelete" }
        });

        public static string GetBlobPermissionKey(string permission)
        {
            return BlobSasPermissions.FirstOrDefault(x => x.Value == permission).Key;
        }

        public static ReadOnlyDictionary<string, string> ContainerSasPermissions = new(new Dictionary<string, string>
        {
            { "r", "Read" },
            { "a", "Add" },
            { "c", "Create" },
            { "w", "Write" },
            { "d", "Delete" },
            { "l", "List" },
            { "AnyPermission", "Any" }
        });

        public static string GetContainerPermissionKey(string permission)
        {
            return ContainerSasPermissions.FirstOrDefault(x => x.Value == permission).Key;
        }
    }

    public class BlobSasResourceType
    {
        public static ReadOnlyDictionary<string, string> BlobSasResourceTypes = new(new Dictionary<string, string>
        {
            { "c", "Container" },
            { "b", "Blob" },
            { "bs", "BlobSnapshot" }
        });

        public static string GetBlobSasResourceTypeKey(string resourceType)
        {
            return BlobSasResourceTypes.FirstOrDefault(x => x.Value == resourceType).Key;
        }
    }
}