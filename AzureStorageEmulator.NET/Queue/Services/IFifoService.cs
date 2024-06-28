using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Results;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public interface IFifoService
    {
        #region QueueOps

        Task<bool> CreateQueueAsync(string queueName, CancellationToken? cancellationToken);

        Task<(IMethodResult, List<Models.Queue>)> ListQueuesAsync(CancellationToken? cancellationToken);

        Task<Models.Queue?> GetQueueMetadataAsync(string queueName);

        Task<bool> DeleteQueueAsync(string queueName);

        #endregion QueueOps

        #region MessageOps

        Task<IMethodResult> PutMessageAsync(string queueName, QueueMessage message,
            CancellationToken? cancellationToken);

        Task<(IMethodResult, List<QueueMessage>?)> GetMessagesAsync(string queueName,
            int? numOfMessages = null,
            bool peekOnly = false,
            CancellationToken? cancellationToken = null);

        Task<(IMethodResult, QueueMessage?)> DeleteMessageAsync(string queueName, Guid messageId, string popReceipt,
            CancellationToken? cancellationToken);

        Task<int> ClearMessagesAsync(string queueName);

        Task<int?> MessageCountAsync(string queueName);

        #endregion MessageOps
    }
}