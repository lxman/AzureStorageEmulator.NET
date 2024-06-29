using System.Collections.Concurrent;

namespace AzureStorageEmulator.NET.Queue.Models
{
    public class QueueObject
    {
        public Queue Queue { get; set; }

        public ConcurrentQueue<QueueMessage> Messages { get; set; } = [];

        public QueueObject(string name)
        {
            Queue = new Queue { Name = name };
        }

        public QueueObject(Queue queue, ConcurrentQueue<QueueMessage> messages)
        {
            Queue = queue;
            Messages = messages;
        }
    }
}
