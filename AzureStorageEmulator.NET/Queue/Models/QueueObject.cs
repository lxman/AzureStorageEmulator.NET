namespace AzureStorageEmulator.NET.Queue.Models
{
    public class QueueObject
    {
        public Queue Queue { get; set; }

        public ConcurrentActiveCountableQueue<QueueMessage> Messages { get; set; } = [];

        public QueueObject(string name)
        {
            Queue = new Queue(name);
        }

        public QueueObject(Queue queue, ConcurrentActiveCountableQueue<QueueMessage> messages)
        {
            lock (new object())
            {
                Messages.Clear();
                messages.ToList().ForEach(Messages.Enqueue);
            }
            Queue = queue;
            Messages = messages;
            Messages.CountChanged += (sender, args) => Queue.MessageCount = Messages.Count;
        }
    }
}
