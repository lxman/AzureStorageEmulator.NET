using Azure.Storage.Queues;
using QueueList;

foreach (string n in Queues.Names)
{
    QueueClient client = new("UseDevelopmentStorage=true", n);
    await client.CreateIfNotExistsAsync();
}