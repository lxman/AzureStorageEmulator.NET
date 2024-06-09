using System.Collections.Concurrent;
using AzureStorageEmulator.NET.Queue.Models;
using Serilog;
using XmlTransformer.Queue.Models;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public class BlockingCollectionService : IFifoService
    {
        private readonly Dictionary<string, BlockingCollection<QueueMessage?>> _queues = [];

        public List<string> GetQueues()
        {
            return [.. _queues.Keys];
        }

        public bool AddQueue(string queueName)
        {
            if (_queues.ContainsKey(queueName)) return false;
            _queues.Add(queueName, new BlockingCollection<QueueMessage?>());
            return true;
        }

        public void DeleteQueue(string queueName)
        {
            _queues.Remove(queueName);
        }

        public void AddMessage(string queueName, QueueMessage message)
        {
            if (!_queues.TryGetValue(queueName, out BlockingCollection<QueueMessage?>? queue)) return;
            queue.Add(message);
        }

        public List<QueueMessage?>? GetMessages(string queueName, int numOfMessages)
        {
            _ = _queues.TryGetValue(queueName, out BlockingCollection<QueueMessage?>? queue) ? queue : null;
            if (queue is null) return null;
            return queue.Count == 0 ? null : queue.Take(numOfMessages).ToList();
        }

        public QueueMessage? GetMessage(string queueName)
        {
            return _queues.TryGetValue(queueName, out BlockingCollection<QueueMessage?>? queue) ? queue.TryTake(out QueueMessage? result) ? result : null : null;
        }

        public async Task<QueueMessage?> DeleteMessage(string queueName, Guid messageId, string popReceipt)
        {
            // Does the queue exist?
            KeyValuePair<string, BlockingCollection<QueueMessage?>> entry = _queues.FirstOrDefault(q => q.Key == queueName);
            if (entry.Value is null) return null;

            // If so remove it from the queue dictionary
            _queues.Remove(queueName);

            while (_queues.ContainsKey(queueName))
            {
                Log.Information($"Waiting for {queueName} to delete");
                await Task.Delay(50);
            }

            // Does the message exist?
            QueueMessage? msg = entry.Value.FirstOrDefault(m => m?.MessageId == messageId);
            if (msg is null) return null;

            // If so, reconstruct a new queue without the message
            List<QueueMessage?> msgList = entry.Value.Where(m => m?.MessageId != messageId).ToList();
            BlockingCollection<QueueMessage?> newQueue = [];
            msgList.ForEach(newQueue.Add);

            _queues.Add(queueName, newQueue);

            return msg;
        }

        public void DeleteMessages(string queueName)
        {
            if (!_queues.TryGetValue(queueName, out BlockingCollection<QueueMessage?>? queue))
            {
                return;
            }

            while (queue.TryTake(out _)) { }
        }
    }
}