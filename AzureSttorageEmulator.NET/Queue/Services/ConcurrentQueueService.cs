using System.Collections.Concurrent;
using AzureStorageEmulator.NET.Queue.Models;
using Serilog;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public class ConcurrentQueueService : IFifoService
    {
        private readonly Dictionary<string, ConcurrentQueue<QueueMessage?>> _queues = [];

        public List<string> GetQueues()
        {
            return [.. _queues.Keys];
        }

        public bool AddQueue(string queueName)
        {
            if (_queues.ContainsKey(queueName)) return false;
            _queues.Add(queueName, new ConcurrentQueue<QueueMessage?>());
            return true;
        }

        public void DeleteQueue(string queueName)
        {
            _queues.Remove(queueName);
        }

        public void AddMessage(string queueName, QueueMessage message)
        {
            if (!_queues.TryGetValue(queueName, out ConcurrentQueue<QueueMessage?>? queue)) return;
            if (queueName == "events") Log.Information($"Message added to events queue. Now contains {queue.Count} messages.");
            queue.Enqueue(message);
        }

        public List<QueueMessage?>? GetMessages(string queueName, int numOfMessages)
        {
            _ = _queues.TryGetValue(queueName, out ConcurrentQueue<QueueMessage?>? queue) ? queue : null;
            if (queue is null) return null;
            return queue.IsEmpty ? null : queue.TakeLast(numOfMessages).ToList();
        }

        public QueueMessage? GetMessage(string queueName)
        {
            return _queues.TryGetValue(queueName, out ConcurrentQueue<QueueMessage?>? queue) ? queue.TryDequeue(out QueueMessage? result) ? result : null : null;
        }

        public async Task<QueueMessage?> DeleteMessage(string queueName, Guid messageId, string popReceipt)
        {
            // Does the queue exist?
            KeyValuePair<string, ConcurrentQueue<QueueMessage?>> entry = _queues.FirstOrDefault(q => q.Key == queueName);
            if (entry.Value is null) return null;

            // If so remove it from the queue dictionary
            _queues.Remove(queueName);

            // Does the message exist?
            QueueMessage? msg = entry.Value.FirstOrDefault(m => m?.MessageId == messageId);
            if (msg is null) return null;

            // If so, reconstruct a new queue without the message
            List<QueueMessage?> newQueue = entry.Value.Where(m => m?.MessageId != messageId).ToList();

            while (_queues.ContainsKey(queueName))
            {
                Log.Information($"Waiting for {queueName} to delete");
                await Task.Delay(20);
            }

            // Add the new queue back to the dictionary
            _queues.Add(queueName, new ConcurrentQueue<QueueMessage?>(newQueue));

            // Return the message
            msg.PopReceipt = popReceipt;
            if (queueName == "events") Log.Information($"Message deleted from events queue. Now contains {newQueue.Count} messages.");
            return msg;
        }
    }
}