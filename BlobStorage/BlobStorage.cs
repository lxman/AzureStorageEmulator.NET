using LiteDB;

namespace BlobStorage
{
    public interface IBlobStorage
    {
        void UploadFile(string fileId, Stream fileStream);

        Stream DownloadFile(string fileName);

        void DeleteFile(string fileName);

        Task Persist(string location);

        Task Restore(string location);

        void Delete(string location);
    }

    public class BlobStorage : IBlobStorage
    {
        private static MemoryStream _backing = new();
        private static readonly ILiteDatabase Db = new LiteDatabase(_backing);
        private readonly ILiteStorage<string>? _fileStorage = Db.FileStorage;

        public void UploadFile(string fileId, Stream fileStream)
        {
            LiteFileInfo<string>? file = _fileStorage?.FindById(fileId);
            if (file is null)
            {
                LiteFileStream<string>? stream = _fileStorage?.OpenWrite(fileId, fileId.Split('/').Last());
                if (stream is not null)
                {
                    fileStream.CopyTo(stream);
                    stream.Close();
                }
            }
            _fileStorage?.Upload(fileId, fileId.Split('/').Last(), fileStream);
        }

        public Stream DownloadFile(string fileId)
        {
            return _fileStorage?.OpenRead(fileId) ?? Stream.Null;
        }

        public void DeleteFile(string fileName)
        {
            _fileStorage?.Delete(fileName);
        }

        public async Task Persist(string location)
        {
            await File.WriteAllBytesAsync(GetSavePath(location), _backing.ToArray());
        }

        public async Task Restore(string location)
        {
            string saveFilePath = GetSavePath(location);
            if (!File.Exists(saveFilePath)) return;
            _backing = new MemoryStream(await File.ReadAllBytesAsync(GetSavePath(location)));
        }

        public void Delete(string location)
        {
            string saveFilePath = GetSavePath(location);
            if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
        }

        private static string GetSavePath(string location) => Path.Combine(location, "AzureStorageEmulator.NET", "Blob", "Blobs.db");
    }
}