using System.Collections.Concurrent;
using AzureStorageEmulator.NET.Queue.Models;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public class ConcurrentQueueService : IFifoService
    {
        private readonly ConcurrentDictionary<Models.Queue, ConcurrentQueue<QueueMessage>> _queues = [];

        public List<Models.Queue> GetQueues()
        {
            return [.. _queues.Keys.OrderBy(k => k.Name)];
        }

        public bool AddQueue(string queueName)
        {
            return _queues.TryAdd(new Models.Queue { Name = queueName }, new ConcurrentQueue<QueueMessage>());
        }

        public async Task<bool> DeleteQueue(string queueName)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return false;
            while (key.Blocked)
            {
                await Task.Delay(50);
            }
            return _queues.TryRemove(key, out _);
        }

        public async Task<bool> AddMessageAsync(string queueName, QueueMessage message)
        {
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueue(queueName);
            queue?.Enqueue(message);
            return queue is not null;
        }

        public async Task<List<QueueMessage>?> GetMessages(string queueName, int? numOfMessages = null,
            bool peekOnly = false)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return null;
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueue(queueName);
            if (queue is null)
            {
                return null;
            }
            key.Blocked = true;
            List<QueueMessage> messages = [.. queue];
            messages.RemoveAll(m => m.Expired);
            if (numOfMessages.HasValue)
            {
                messages = messages.Where(m => m.Visible).Take(numOfMessages.Value).ToList();
            }

            if (!peekOnly)
            {
                _queues.TryUpdate(key, new ConcurrentQueue<QueueMessage>(messages), queue);
            }

            key.Blocked = false;
            return messages;
        }

        public List<QueueMessage>? GetAllMessages(string queueName)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return null;
            key.Blocked = true;
            List<QueueMessage> messages = [.. _queues[key]];
            messages.RemoveAll(m => m.Expired);
            List<QueueMessage> toReturn = messages.Where(m => m.Visible).ToList();
            toReturn.ForEach(m => messages.Remove(m));
            _queues.TryUpdate(key, new ConcurrentQueue<QueueMessage>(messages), _queues[key]);
            key.Blocked = false;
            return toReturn;
        }

        public async Task<QueueMessage?> DeleteMessage(string queueName, Guid messageId, string popReceipt)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return null;
            key.Blocked = true;
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueue(queueName);
            if (queue is null)
            {
                key.Blocked = false;
                return null;
            }
            List<QueueMessage> messages = queue.ToList() ?? [];
            QueueMessage? message = messages.FirstOrDefault(m => m?.MessageId == messageId && m.PopReceipt == popReceipt && m.Visible);
            if (message is null)
            {
                key.Blocked = false;
                return null;
            }
            messages.Remove(message);
            _queues.TryUpdate(key, new ConcurrentQueue<QueueMessage>(messages), queue);
            key.Blocked = false;
            return message;
        }

        public void DeleteMessages(string queueName)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return;
            key.Blocked = true;
            _queues.TryUpdate(key, new ConcurrentQueue<QueueMessage>(), new ConcurrentQueue<QueueMessage>());
            key.Blocked = false;
        }

        public async Task<int?> MessageCount(string queueName)
        {
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueue(queueName);
            return queue?.Count;
        }

        private async Task<ConcurrentQueue<QueueMessage>?> TryGetQueue(string queueName)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return null;
            while (key.Blocked)
            {
                await Task.Delay(50);
            }
            ConcurrentQueue<QueueMessage>? queue = _queues!.GetValueOrDefault(_queues.Keys.FirstOrDefault(q => q.Name == queueName));
            return queue;
        }
    }
}
