namespace AzureStorageEmulator.NET.Blob.Models
{
    public interface IFolder
    {
        public IContainer? ContainerParent { get; set; }

        public IFolder? FolderParent { get; set; }

        string Name { get; set; }

        List<Blob> Blobs { get; set; }

        List<IFolder> Folders { get; set; }
    }

    public class Folder : IFolder
    {
        public IContainer? ContainerParent { get; set; }

        public IFolder? FolderParent { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<Blob> Blobs { get; set; } = [];

        public List<IFolder> Folders { get; set; } = [];
    }
}