using System.Collections.Concurrent;
using AzureStorageEmulator.NET.Queue.Models;
using Serilog;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public class BlockingCollectionService : IFifoService
    {
        private readonly Dictionary<Models.Queue, BlockingCollection<QueueMessage?>> _queues = [];

        public List<Models.Queue> GetQueues()
        {
            return [.. _queues.Keys];
        }

        public bool AddQueue(string queueName)
        {
            if (QueueNames().Contains(queueName)) return false;
            _queues.Add(new Models.Queue { Name = queueName }, []);
            return true;
        }

        public void DeleteQueue(string queueName)
        {
            if (!TryGetQueue(queueName, out KeyValuePair<Models.Queue, BlockingCollection<QueueMessage?>>? queue)) return;
            _queues.Remove(queue!.Value.Key);
        }

        public void AddMessage(string queueName, QueueMessage message)
        {
            _ = TryGetQueue(queueName, out KeyValuePair<Models.Queue, BlockingCollection<QueueMessage?>>? queue);
            queue?.Value.Add(message);
        }

        public List<QueueMessage?>? GetMessages(string queueName, int numOfMessages)
        {
            return !TryGetQueue(queueName, out KeyValuePair<Models.Queue, BlockingCollection<QueueMessage?>>? queue)
                ? null
                : FilterMessagesByTime(queue!.Value.Value.Count == 0 ? null : queue.Value.Value.Take(numOfMessages).ToList());
        }

        public List<QueueMessage?>? GetAllMessages(string queueName)
        {
            return !TryGetQueue(queueName,
                out KeyValuePair<Models.Queue, BlockingCollection<QueueMessage?>>? queue)
                ? null
                : FilterMessagesByTime([.. queue!.Value.Value]);
        }

        public async Task<QueueMessage?> DeleteMessage(string queueName, Guid messageId, string popReceipt)
        {
            // Does the queue exist?
            if (!TryGetQueue(queueName, out KeyValuePair<Models.Queue, BlockingCollection<QueueMessage?>>? queue)) return null;

            // If so remove it from the queue dictionary
            _queues.Remove(queue!.Value.Key);

            while (_queues.ContainsKey(queue.Value.Key))
            {
                Log.Information($"Waiting for {queueName} to delete");
                await Task.Delay(50);
            }

            // Does the message exist?
            QueueMessage? msg = queue.Value.Value.FirstOrDefault(m => m?.MessageId == messageId && m.PopReceipt == popReceipt);
            if (msg is null) return null;

            // If so, reconstruct a new queue without the message
            List<QueueMessage?> msgList = queue.Value.Value.Where(m => m?.MessageId != messageId).ToList();
            BlockingCollection<QueueMessage?> newList = [];
            msgList.ForEach(newList.Add);
            Models.Queue newQueue = new() { Name = queueName, Metadata = queue.Value.Key.Metadata };

            _queues.Add(newQueue, newList);

            return msg;
        }

        public void DeleteMessages(string queueName)
        {
            if (!TryGetQueue(queueName, out KeyValuePair<Models.Queue, BlockingCollection<QueueMessage?>>? queue)) return;
            while (queue!.Value.Value.TryTake(out _)) { }
        }

        public int MessageCount(string queueName)
        {
            return !TryGetQueue(queueName, out KeyValuePair<Models.Queue, BlockingCollection<QueueMessage?>>? queue)
                ? 0
                : queue!.Value.Value.Count;
        }

        private List<string> QueueNames() => _queues.Keys.Select(k => k.Name).ToList();

        private bool TryGetQueue(
            string queueName,
            out KeyValuePair<Models.Queue, BlockingCollection<QueueMessage?>>? queue)
        {
            queue = _queues.FirstOrDefault(q => q.Key.Name == queueName);
            return queue is not null;
        }

        private static List<QueueMessage?>? FilterMessagesByTime(List<QueueMessage?>? messages)
        {
            return messages?.Where(m => m?.TimeNextVisible >= DateTime.UtcNow).ToList();
        }
    }
}