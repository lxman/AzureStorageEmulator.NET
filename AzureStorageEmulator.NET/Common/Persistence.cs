using AzureStorageEmulator.NET.Blob.Services;
using AzureStorageEmulator.NET.Queue.Services;
using AzureStorageEmulator.NET.Table.Services;

namespace AzureStorageEmulator.NET.Common
{
    public class Persistence
    {
        public static async Task Persist(
            PersistenceSettings settings,
            IQueueService queueService,
            ITableService tableService,
            IBlobService blobService
            )
        {
            string rootPath = settings.RootPath;
            if (settings.Table)
            {
                await tableService.Persist(rootPath);
            }

            if (settings.Queue)
            {
                await queueService.Persist(rootPath);
            }

            if (settings.Blob)
            {
                await blobService.Persist(rootPath);
            }
        }

        public static async Task Restore(
            string rootPath,
            IQueueService queueService,
            ITableService tableService,
            IBlobService blobService
            )
        {
            await tableService.Restore(rootPath);
            await queueService.Restore(rootPath);
            await blobService.Restore(rootPath);
        }

        public static void Delete(
            PersistenceSettings settings,
            IQueueService queueService,
            ITableService tableService,
            IBlobService blobService
            )
        {
            string rootPath = settings.RootPath;
            if (settings.ClearTable)
            {
                tableService.Delete(rootPath);
            }

            if (settings.ClearQueue)
            {
                queueService.Delete(rootPath);
            }

            if (settings.ClearBlob)
            {
                blobService.Delete(rootPath);
            }
        }
    }
}