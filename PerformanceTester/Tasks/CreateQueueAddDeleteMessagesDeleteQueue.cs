using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace PerformanceTester.Tasks
{
    internal class CreateQueueAddDeleteMessagesDeleteQueue
    {
        internal static async Task Run()
        {
            Console.WriteLine("Running Create Queue Add/Delete Messages Delete Queue test...");
            QueueClient client = new("UseDevelopmentStorage=true", "accounting-events");
            Console.WriteLine("Creating queue: accounting-events");
            await client.CreateIfNotExistsAsync();
            List<string> messages = Enumerable.Range(0, 4096).Select(_ => Guid.NewGuid().ToString()).ToList();
            messages.ForEach(async message =>
            {
                Console.WriteLine($"Adding message: {message}");
                Response<SendReceipt>? result = await client.SendMessageAsync(message);
                Console.WriteLine($"Message sent. Id: {result.Value.MessageId}, PopReceipt: {result.Value.PopReceipt}");
            });
            Console.WriteLine("Clearing queue accounting-events");
            await client.ClearMessagesAsync();
            Console.WriteLine("Deleting queue accounting-events");
            await client.DeleteAsync();
        }
    }
}
