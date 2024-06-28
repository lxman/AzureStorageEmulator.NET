using System.Collections.Concurrent;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Results;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public class ConcurrentQueueService : IFifoService
    {
        private readonly ConcurrentDictionary<Models.Queue, ConcurrentQueue<QueueMessage>> _queues = [];

        #region QueueOps

        public async Task<bool> CreateQueueAsync(string queueName, CancellationToken? cancellationToken)
        {
            await RemoveExpired(queueName);
            if (cancellationToken is { IsCancellationRequested: true }) return false;
            return _queues.Keys.All(q => q.Name != queueName) && _queues.TryAdd(new Models.Queue { Name = queueName }, new ConcurrentQueue<QueueMessage>());
        }

        public async Task<(IMethodResult, List<Models.Queue>)> ListQueuesAsync(CancellationToken? cancellationToken)
        {
            List<Models.Queue> keys = [.. _queues.Keys.OrderBy(k => k.Name)];
            foreach (Models.Queue queue in keys)
            {
                await RemoveExpired(queue.Name);
            };
            if (cancellationToken is { IsCancellationRequested: true }) return (new ResultTimeout(), []);
            return (new ResultOk(), keys);
        }

        public async Task<Models.Queue?> GetQueueMetadataAsync(string queueName)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (key is null) return null;
            await RemoveExpired(queueName);
            key.MessageCount = _queues.GetValueOrDefault(key)?.Count ?? 0;
            return key;
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

        #endregion QueueOps

        #region MessageOps

        public async Task<IMethodResult> PutMessageAsync(string queueName, QueueMessage message,
            CancellationToken? cancellationToken)
        {
            await RemoveExpired(queueName);
            if (cancellationToken is { IsCancellationRequested: true }) return new ResultTimeout();
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueueAsync(queueName);
            if (cancellationToken is { IsCancellationRequested: true }) return new ResultTimeout();
            queue?.Enqueue(message);
            return queue is not null ? new ResultOk() : new ResultNotFound();
        }

        public async Task<(IMethodResult, List<QueueMessage>?)> GetMessagesAsync(string queueName,
            int? numOfMessages,
            bool peekOnly = false, CancellationToken? cancellationToken = null)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (cancellationToken is { IsCancellationRequested: true })
            {
                return GetMessagesTimeout();
            }

            if (key is null)
            {
                return GetMessagesNotFound();
            }
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueueAsync(queueName);
            if (cancellationToken is { IsCancellationRequested: true })
            {
                return GetMessagesTimeout();
            }
            if (queue is null)
            {
                return GetMessagesNotFound();
            }
            key.Blocked = true;
            List<QueueMessage> messages = [.. queue];
            messages.RemoveAll(m => m.Expired);
            if (cancellationToken is { IsCancellationRequested: true })
            {
                key.Blocked = false;
                return GetMessagesTimeout();
            }
            if (numOfMessages.HasValue)
            {
                messages = messages.Where(m => m.Visible).Take(numOfMessages.Value).ToList();
            }

            if (cancellationToken is { IsCancellationRequested: true })
            {
                key.Blocked = false;
                return GetMessagesTimeout();
            }
            if (!peekOnly)
            {
                List<QueueMessage> toRetain = [.. queue];
                messages.ForEach(m => toRetain.Remove(m));
                toRetain.RemoveAll(m => m.Expired);
                _queues.TryUpdate(key, new ConcurrentQueue<QueueMessage>(toRetain), queue);
                if (cancellationToken is { IsCancellationRequested: true })
                {
                    key.Blocked = false;
                    return GetMessagesTimeout();
                }
                messages.ForEach(m =>
                {
                    m.DequeueCount++;
                    m.PopReceipt = Guid.NewGuid().ToString();
                    m.InsertionTime = DateTime.UtcNow;
                });
            }

            key.Blocked = false;
            return (new ResultOk(), messages);
        }

        public async Task<(IMethodResult, QueueMessage?)> DeleteMessageAsync(string queueName, Guid messageId,
            string popReceipt,
            CancellationToken? cancellationToken)
        {
            Models.Queue? key = _queues.Keys.FirstOrDefault(q => q.Name == queueName);
            if (cancellationToken is { IsCancellationRequested: true })
            {
                return DeleteMessageTimeout();
            }
            if (key is null) return DeleteMessageNotFound();
            ConcurrentQueue<QueueMessage>? queue = await TryGetQueueAsync(queueName);
            if (cancellationToken is { IsCancellationRequested: true })
            {
                return DeleteMessageTimeout();
            }
            if (queue is null)
            {
                return DeleteMessageNotFound();
            }
            await RemoveExpired(queueName);
            if (cancellationToken is { IsCancellationRequested: true })
            {
                return DeleteMessageTimeout();
            }
            key.Blocked = true;
            List<QueueMessage> messages = queue.ToList() ?? [];
            QueueMessage? message = messages.FirstOrDefault(m => m?.MessageId == messageId && m.PopReceipt == popReceipt);
            if (message is null)
            {
                key.Blocked = false;
                return DeleteMessageNotFound();
            }
            messages.Remove(message);
            _queues.TryUpdate(key, new ConcurrentQueue<QueueMessage>(messages), queue);
            key.Blocked = false;
            return (new ResultOk(), message);
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

        #endregion MessageOps

        #region Private Helpers

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

        private static (IMethodResult, List<QueueMessage>?) GetMessagesTimeout() => (new ResultTimeout(), null);

        private static (IMethodResult, List<QueueMessage>?) GetMessagesNotFound() => (new ResultNotFound(), null);

        private static (IMethodResult, QueueMessage?) DeleteMessageTimeout() => (new ResultTimeout(), null);

        private static (IMethodResult, QueueMessage?) DeleteMessageNotFound() => (new ResultNotFound(), null);

        #endregion Private Helpers
    }
}