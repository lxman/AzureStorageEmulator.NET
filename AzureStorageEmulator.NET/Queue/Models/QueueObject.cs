namespace AzureStorageEmulator.NET.Queue.Models
{
    public class QueueObject
    {
        public QueueMetadata QueueMetadata { get; set; }

        public ConcurrentActiveCountableQueue<QueueMessage> Messages { get; set; } = [];

        public QueueObject(string name)
        {
            QueueMetadata = new QueueMetadata(name);
        }

        public QueueObject(QueueMetadata queueMetadata, ConcurrentActiveCountableQueue<QueueMessage> messages)
        {
            lock (new object())
            {
                Messages.Clear();
                messages.ToList().ForEach(Messages.Enqueue);
            }
            QueueMetadata = queueMetadata;
            Messages = messages;
            Messages.CountChanged += (sender, args) => QueueMetadata.MessageCount = Messages.Count;
        }
    }
}
