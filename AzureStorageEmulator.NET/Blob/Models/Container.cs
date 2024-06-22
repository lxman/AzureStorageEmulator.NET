namespace AzureStorageEmulator.NET.Blob.Models
{
    public interface IContainer
    {
        string Name { get; set; }
        List<Folder> Folders { get; set; }
        List<Blob> Blobs { get; set; }
    }

    public class Container : IContainer
    {
        public string Name { get; set; } = string.Empty;

        public List<Folder> Folders { get; set; } = [];

        public List<Blob> Blobs { get; set; } = [];

        public Container(string name)
        {
            Name = name;
        }
    }
}
