using System.Collections.Concurrent;
using System.Text.Json;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Results;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public class ConcurrentQueueService : IFifoService
    {
        private readonly ConcurrentDictionary<Guid, QueueObject> _queues = [];

        #region QueueOps

        public bool CreateQueueAsync(string queueName)
        {
            return _queues.All(q => q.Value.QueueMetadata.Name != queueName)
                   && _queues.TryAdd(Guid.NewGuid(), new QueueObject(queueName));
        }

        public async Task<(IMethodResult, List<QueueMetadata>)> ListQueuesAsync(CancellationToken? cancellationToken)
        {
            List<QueueObject> keys = [.. _queues.Values];
            foreach (QueueObject queue in keys)
            {
                await RemoveExpired(queue.QueueMetadata.Name);
            }
            if (cancellationToken is { IsCancellationRequested: true }) return (new ResultTimeout(), []);
            return (new ResultOk(), [.. _queues.Values.Select(q => q.QueueMetadata)]);
        }

        public async Task<(IMethodResult, QueueMetadata?)> GetQueueMetadataAsync(string queueName,
            CancellationToken? cancellationToken)
        {
            QueueObject? queueObject = _queues.Values.FirstOrDefault(q => q.QueueMetadata.Name == queueName);
            if (queueObject is null) return (new ResultNotFound(), null);
            await RemoveExpired(queueName);
            if (cancellationToken is { IsCancellationRequested: true }) return (new ResultTimeout(), null);
            return (new ResultOk(), queueObject.QueueMetadata);
        }

        public async Task<IMethodResult> DeleteQueueAsync(string queueName, CancellationToken? cancellationToken)
        {
            KeyValuePair<Guid, QueueObject>? key = await TryGetEntryByNameAsync(queueName);
            if (key is null) return new ResultNotFound();
            while (key.Value.Value.QueueMetadata.Blocked)
            {
                if (cancellationToken is { IsCancellationRequested: true }) return new ResultTimeout();
                await Task.Delay(50);
            }
            return _queues.TryRemove(key.Value.Key, out _) ? new ResultOk() : new ResultGone();
        }

        #endregion QueueOps

        #region MessageOps

        public async Task<IMethodResult> PutMessageAsync(string queueName, QueueMessage message,
            CancellationToken? cancellationToken)
        {
            await RemoveExpired(queueName);
            if (cancellationToken is { IsCancellationRequested: true }) return new ResultTimeout();
            KeyValuePair<Guid, QueueObject>? entry = await TryGetEntryByNameAsync(queueName);
            if (entry is null) return new ResultNotFound();
            if (cancellationToken is { IsCancellationRequested: true }) return new ResultTimeout();
            entry.Value.Value.Messages.Enqueue(message);
            return new ResultOk();
        }

        public async Task<(IMethodResult, List<QueueMessage>?)> GetMessagesAsync(string queueName,
            int? numOfMessages,
            bool peekOnly = false, CancellationToken? cancellationToken = null)
        {
            numOfMessages ??= 1;
            if (numOfMessages is null or < 1) return (new ResultOk(), null);
            KeyValuePair<Guid, QueueObject>? entry = await TryGetEntryByNameAsync(queueName);
            if (cancellationToken is { IsCancellationRequested: true })
            {
                return GetMessagesTimeout();
            }
            if (entry is null)
            {
                return GetMessagesNotFound();
            }
            if (cancellationToken is { IsCancellationRequested: true })
            {
                return GetMessagesTimeout();
            }
            entry.Value.Value.QueueMetadata.Blocked = true;
            List<QueueMessage> allMessages = [.. entry.Value.Value.Messages];
            allMessages.RemoveAll(m => m.Expired);
            if (cancellationToken is { IsCancellationRequested: true })
            {
                entry.Value.Value.QueueMetadata.Blocked = false;
                return GetMessagesTimeout();
            }

            if (cancellationToken is { IsCancellationRequested: true })
            {
                entry.Value.Value.QueueMetadata.Blocked = false;
                return GetMessagesTimeout();
            }
            if (!peekOnly)
            {
                foreach (QueueMessage m in allMessages.Where(m => m.Visible).Take(numOfMessages.Value))
                {
                    m.DequeueCount++;
                    m.PopReceipt = Guid.NewGuid().ToString();
                    m.InsertionTime = DateTime.UtcNow;
                }
            }
            entry.Value.Value.QueueMetadata.Blocked = false;
            QueueObject newQueue = new(entry.Value.Value.QueueMetadata, new ConcurrentActiveCountableQueue<QueueMessage>(allMessages));
            _queues.TryUpdate(entry.Value.Key, newQueue, entry.Value.Value);

            return (new ResultOk(), allMessages.Where(m => m.Visible).Take(numOfMessages.Value).ToList());
        }

        public async Task<(IMethodResult, QueueMessage?)> DeleteMessageAsync(string queueName, Guid messageId,
            string popReceipt,
            CancellationToken? cancellationToken)
        {
            KeyValuePair<Guid, QueueObject>? entry = await TryGetEntryByNameAsync(queueName);
            if (cancellationToken is { IsCancellationRequested: true })
            {
                return DeleteMessageTimeout();
            }
            if (entry is null) return DeleteMessageNotFound();
            if (cancellationToken is { IsCancellationRequested: true })
            {
                return DeleteMessageTimeout();
            }
            await RemoveExpired(queueName);
            if (cancellationToken is { IsCancellationRequested: true })
            {
                return DeleteMessageTimeout();
            }
            entry.Value.Value.QueueMetadata.Blocked = true;
            List<QueueMessage> messages = [.. entry.Value.Value.Messages];
            QueueMessage? message = messages.FirstOrDefault(m => m.MessageId == messageId && m.PopReceipt == popReceipt);
            if (message is null)
            {
                entry.Value.Value.QueueMetadata.Blocked = false;
                return DeleteMessageNotFound();
            }
            bool result = messages.Remove(message);
            entry.Value.Value.QueueMetadata.Blocked = false;
            // For the life of me, I don't understand why TryUpdate is not working here.
            lock (new object())
            {
                _queues[entry.Value.Key] = new QueueObject(entry.Value.Value.QueueMetadata, new ConcurrentActiveCountableQueue<QueueMessage>(messages));
            }
            return (new ResultOk(), message);
        }

        public async Task<int> ClearMessagesAsync(string queueName, CancellationToken? cancellationToken)
        {
            KeyValuePair<Guid, QueueObject>? entry = await TryGetEntryByNameAsync(queueName);
            Guid? key = _queues.FirstOrDefault(q => q.Value.QueueMetadata.Name == queueName).Key;
            if (entry is null) return 404;
            if (cancellationToken is { IsCancellationRequested: true }) return 504;

            return _queues.TryUpdate(entry.Value.Key, new QueueObject(queueName), entry.Value.Value) ? 204 : 409;
        }

        public async Task<int?> MessageCountAsync(string queueName)
        {
            await RemoveExpired(queueName);
            KeyValuePair<Guid, QueueObject>? entry = await TryGetEntryByNameAsync(queueName);
            return entry?.Value.QueueMetadata.MessageCount;
        }

        #endregion MessageOps

        #region Persistence

        public async Task Persist(string location)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(location, "AzureStorageEmulator.NET", "Queue"));
                string saveFilePath = GetSavePath(location);
                if (!_queues.IsEmpty) await File.WriteAllTextAsync(saveFilePath, JsonSerializer.Serialize(_queues));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task Restore(string location)
        {
            string saveFilePath = GetSavePath(location);
            if (!File.Exists(saveFilePath)) return;
            string json = await File.ReadAllTextAsync(saveFilePath);
            ConcurrentDictionary<Guid, QueueObject> queues = JsonSerializer.Deserialize<ConcurrentDictionary<Guid, QueueObject>>(json) ?? [];
            foreach (KeyValuePair<Guid, QueueObject> queue in queues)
            {
                _queues.TryAdd(queue.Key, queue.Value);
            }
        }

        public void Delete(string location)
        {
            string saveFilePath = GetSavePath(location);
            if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
        }

        #endregion Persistence

        #region Private Helpers

        private async Task RemoveExpired(string queueName)
        {
            KeyValuePair<Guid, QueueObject>? entry = await TryGetEntryByNameAsync(queueName);
            if (entry is null) return;
            ConcurrentQueue<QueueMessage> queue = entry.Value.Value.Messages;
            entry.Value.Value.QueueMetadata.Blocked = true;
            List<QueueMessage> messages = queue.ToList() ?? [];
            messages.RemoveAll(m => m.Expired);
            entry.Value.Value.QueueMetadata.Blocked = false;
            _queues.TryUpdate(entry.Value.Key, new QueueObject(entry.Value.Value.QueueMetadata, new ConcurrentActiveCountableQueue<QueueMessage>(messages)), entry.Value.Value);
        }

        private static string GetSavePath(string location) => Path.Combine(location, "AzureStorageEmulator.NET", "Queue", "Queues.json");

        private async Task<KeyValuePair<Guid, QueueObject>?> TryGetEntryByNameAsync(string queueName)
        {
            KeyValuePair<Guid, QueueObject>? key = _queues.FirstOrDefault(q => q.Value.QueueMetadata.Name == queueName);
            if (key.Value.Value is null) return null;
            while (key.Value.Value.QueueMetadata.Blocked)
            {
                await Task.Delay(50);
            }
            return key;
        }

        private static (IMethodResult, List<QueueMessage>?) GetMessagesTimeout() => (new ResultTimeout(), null);

        private static (IMethodResult, List<QueueMessage>?) GetMessagesNotFound() => (new ResultNotFound(), null);

        private static (IMethodResult, QueueMessage?) DeleteMessageTimeout() => (new ResultTimeout(), null);

        private static (IMethodResult, QueueMessage?) DeleteMessageNotFound() => (new ResultNotFound(), null);

        #endregion Private Helpers
    }
}