using System.Collections.Concurrent;
using Serilog;
using XmlTransformer.Queue.Models;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public class BlockingCollectionService : IFifoService
    {
        private readonly Dictionary<XmlTransformer.Queue.Models.Queue, BlockingCollection<QueueMessage?>> _queues = [];

        public List<XmlTransformer.Queue.Models.Queue> GetQueues()
        {
            return [.. _queues.Keys];
        }

        public bool AddQueue(string queueName)
        {
            if (QueueNames().Contains(queueName)) return false;
            _queues.Add(new XmlTransformer.Queue.Models.Queue { Name = queueName }, []);
            return true;
        }

        public void DeleteQueue(string queueName)
        {
            if (!TryGetQueue(queueName, out KeyValuePair<XmlTransformer.Queue.Models.Queue, BlockingCollection<QueueMessage?>>? queue)) return;
            _queues.Remove(queue!.Value.Key);
        }

        public void AddMessage(string queueName, QueueMessage message)
        {
            _ = TryGetQueue(queueName, out KeyValuePair<XmlTransformer.Queue.Models.Queue, BlockingCollection<QueueMessage?>>? queue);
            queue?.Value.Add(message);
        }

        public List<QueueMessage?>? GetMessages(string queueName, int numOfMessages)
        {
            if (!TryGetQueue(queueName, out KeyValuePair<XmlTransformer.Queue.Models.Queue, BlockingCollection<QueueMessage?>>? queue)) return null;
            return queue!.Value.Value.Count == 0 ? null : queue.Value.Value.Take(numOfMessages).ToList();
        }

        public QueueMessage? GetMessage(string queueName)
        {
            return TryGetQueue(queueName, out KeyValuePair<XmlTransformer.Queue.Models.Queue, BlockingCollection<QueueMessage?>>? queue)
                ? queue!.Value.Value.TryTake(out QueueMessage? result)
                    ? result
                    : null
                : null;
        }

        public async Task<QueueMessage?> DeleteMessage(string queueName, Guid messageId, string popReceipt)
        {
            // Does the queue exist?
            if (!TryGetQueue(queueName, out KeyValuePair<XmlTransformer.Queue.Models.Queue, BlockingCollection<QueueMessage?>>? queue)) return null;

            // If so remove it from the queue dictionary
            _queues.Remove(queue!.Value.Key);

            while (_queues.ContainsKey(queue.Value.Key))
            {
                Log.Information($"Waiting for {queueName} to delete");
                await Task.Delay(50);
            }

            // Does the message exist?
            QueueMessage? msg = queue.Value.Value.FirstOrDefault(m => m?.MessageId == messageId);
            if (msg is null) return null;

            // If so, reconstruct a new queue without the message
            List<QueueMessage?> msgList = queue.Value.Value.Where(m => m?.MessageId != messageId).ToList();
            BlockingCollection<QueueMessage?> newList = [];
            msgList.ForEach(newList.Add);
            XmlTransformer.Queue.Models.Queue newQueue = new() { Name = queueName, Metadata = queue.Value.Key.Metadata };

            _queues.Add(newQueue, newList);

            return msg;
        }

        public void DeleteMessages(string queueName)
        {
            if (!TryGetQueue(queueName, out KeyValuePair<XmlTransformer.Queue.Models.Queue, BlockingCollection<QueueMessage?>>? queue)) return;
            while (queue!.Value.Value.TryTake(out _)) { }
        }

        private List<string> QueueNames() => _queues.Keys.Select(k => k.Name).ToList();

        private bool TryGetQueue(
            string queueName,
            out KeyValuePair<XmlTransformer.Queue.Models.Queue, BlockingCollection<QueueMessage?>>? queue)
        {
            queue = _queues.FirstOrDefault(q => q.Key.Name == queueName);
            return queue is not null;
        }
    }
}