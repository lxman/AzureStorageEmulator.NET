// "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator\azurite.exe"

using AppliedQueueList;
using Azure.Storage.Queues;

foreach (string n in Queues.Names)
{
    QueueClient client = new("UseDevelopmentStorage=true", n);
    await client.CreateIfNotExistsAsync();
}