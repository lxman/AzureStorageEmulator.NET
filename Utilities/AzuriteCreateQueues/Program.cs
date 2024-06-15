using AppliedQueueList;
using Azure.Storage.Queues;

foreach (string n in Queues.Names)
{
    QueueClient client = new("UseDevelopmentStorage=true", n);
    await client.CreateIfNotExistsAsync();
}