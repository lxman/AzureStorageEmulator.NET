﻿using AzureStorageEmulator.NET.Common;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Results;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public interface IFifoService : IStorageProvider
    {
        #region QueueOps

        bool CreateQueueAsync(string queueName);

        Task<(IMethodResult, List<QueueMetadata>)> ListQueuesAsync(CancellationToken? cancellationToken);

        Task<(IMethodResult, QueueMetadata?)> GetQueueMetadataAsync(string queueName,
            CancellationToken? cancellationToken);

        Task<IMethodResult> DeleteQueueAsync(string queueName, CancellationToken? cancellationToken);

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

        Task<int> ClearMessagesAsync(string queueName, CancellationToken? cancellationToken);

        Task<int?> MessageCountAsync(string queueName);

        #endregion MessageOps
    }
}