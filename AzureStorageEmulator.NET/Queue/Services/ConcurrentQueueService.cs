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

        public async Task<bool> AddMessageAsync(string queueName, QueueMessage message)
        {
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
                    m.PopReceipt = Guid.NewGuid().ToString();
                    m.TimeNextVisible = DateTime.UtcNow.AddSeconds(m.VisibilityTimeout);
                });
            }

            key.Blocked = false;
            return messages;
        }

        public Models.Queue? GetQueueMetadata(string queueName)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return null;
            key.MessageCount = _queues.GetValueOrDefault(key)?.Count ?? 0;
            return key;
        }

        public async Task<QueueMessage?> DeleteMessageAsync(string queueName, Guid messageId, string popReceipt)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return null;
            key.Blocked = true;
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueueAsync(queueName);
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

        public async Task<int?> MessageCountAsync(string queueName)
        {
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueueAsync(queueName);
            return queue?.Count;
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