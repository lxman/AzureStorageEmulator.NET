using System.Collections.Concurrent;
using AzureStorageEmulator.NET.Queue.Models;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public class ConcurrentQueueService : IFifoService
    {
        private readonly ConcurrentDictionary<Models.Queue, ConcurrentQueue<QueueMessage>> _queues = [];

        public async Task<List<Models.Queue>> ListQueuesAsync()
        {
            List<Models.Queue> keys = [.. _queues.Keys.OrderBy(k => k.Name)];
            foreach (Models.Queue queue in keys)
            {
                await RemoveExpired(queue.Name);
            };
            return keys;
        }

        public async Task<bool> CreateQueueAsync(string queueName)
        {
            await RemoveExpired(queueName);
            return _queues.Keys.All(q => q.Name != queueName) && _queues.TryAdd(new Models.Queue { Name = queueName }, new ConcurrentQueue<QueueMessage>());
        }

        public async Task<bool> DeleteQueueAsync(string queueName)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return false;
            while (key.Blocked)
            {
                await Task.Delay(50);
            }
            return _queues.TryRemove(key, out _);
        }

        public async Task<bool> PutMessageAsync(string queueName, QueueMessage message)
        {
            await RemoveExpired(queueName);
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueueAsync(queueName);
            queue?.Enqueue(message);
            return queue is not null;
        }

        public async Task<List<QueueMessage>?> GetMessagesAsync(string queueName, int? numOfMessages = null,
            bool peekOnly = false)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return null;
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueueAsync(queueName);
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
                List<QueueMessage> toRetain = [.. queue];
                messages.ForEach(m => toRetain.Remove(m));
                toRetain.RemoveAll(m => m.Expired);
                _queues.TryUpdate(key, new ConcurrentQueue<QueueMessage>(toRetain), queue);
                messages.ForEach(m =>
                {
                    m.DequeueCount++;
                    m.PopReceipt = Guid.NewGuid().ToString();
                    m.InsertionTime = DateTime.UtcNow;
                });
            }

            key.Blocked = false;
            return messages;
        }

        public async Task<Models.Queue?> GetQueueMetadataAsync(string queueName)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return null;
            await RemoveExpired(queueName);
            key.MessageCount = _queues.GetValueOrDefault(key)?.Count ?? 0;
            return key;
        }

        public async Task<QueueMessage?> DeleteMessageAsync(string queueName, Guid messageId, string popReceipt)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return null;
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueueAsync(queueName);
            if (queue is null)
            {
                return null;
            }
            await RemoveExpired(queueName);
            key.Blocked = true;
            List<QueueMessage> messages = queue.ToList() ?? [];
            QueueMessage? message = messages.FirstOrDefault(m => m?.MessageId == messageId && m.PopReceipt == popReceipt);
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

        public async Task<int> ClearMessagesAsync(string queueName)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return 404;

            return _queues.TryUpdate(key, new ConcurrentQueue<QueueMessage>(), (await TryGetQueueAsync(queueName))!) ? 204 : 409;
        }

        public async Task<int?> MessageCountAsync(string queueName)
        {
            await RemoveExpired(queueName);
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueueAsync(queueName);
            return queue?.Count;
        }

        private async Task RemoveExpired(string queueName)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return;
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueueAsync(queueName);
            if (queue is null)
            {
                return;
            }
            key.Blocked = true;
            List<QueueMessage> messages = queue.ToList() ?? [];
            messages.RemoveAll(m => m.Expired);
            _queues.TryUpdate(key, new ConcurrentQueue<QueueMessage>(messages), queue);
            key.Blocked = false;
        }

        private async Task<ConcurrentQueue<QueueMessage>?> TryGetQueueAsync(string queueName)
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