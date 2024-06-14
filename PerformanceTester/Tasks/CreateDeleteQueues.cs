using Azure.Storage.Queues;

namespace PerformanceTester.Tasks
{
    internal class CreateDeleteQueues
    {
        internal async Task Run()
        {
            Console.WriteLine("Running Create/Delete Queues test...");
            List<string> tableNames = Enumerable.Range(0, 1024).Select(_ => GetRandomTableName()).ToList();
            await Task.Run(async () =>
            {
                foreach (string tableName in tableNames)
                {
                    Console.WriteLine($"Creating/Deleting queue: {tableName}");
                    QueueClient client = new("UseDevelopmentStorage=true", tableName);
                    await client.CreateIfNotExistsAsync();
                    await client.DeleteAsync();
                }
            });
        }

        private static string GetRandomTableName()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}