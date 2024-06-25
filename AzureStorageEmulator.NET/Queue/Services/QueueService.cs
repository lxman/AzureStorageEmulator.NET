using AzureStorageEmulator.NET.Common;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.XmlSerialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Serilog;

// ReSharper disable UnusedVariable
#pragma warning disable IDE0059

namespace AzureStorageEmulator.NET.Queue.Services
{
    public interface IQueueService
    {
        Task<IActionResult> CreateQueueAsync(string queueName, HttpContext context);

        Task<IActionResult> GetQueuesAsync(CancellationToken? cancellationToken, HttpContext context);

        Task<IActionResult> DeleteQueueAsync(string queueName, HttpContext context);

        Task<IActionResult> PostMessageAsync(string queueName, QueueMessage message, int visibilityTimeout, int messageTtl, int timeout, HttpContext context);

        Task<IActionResult> GetMessagesAsync(string queueName, HttpContext context);

        Task<IActionResult> GetQueueMetadataAsync(string queueName, HttpContext context);

        Task<IActionResult> DeleteMessageAsync(string queueName, Guid messageId, string popReceipt, HttpContext context);

        Task<IActionResult> ClearMessagesAsync(string queueName, HttpContext context);

        Task<IActionResult> MessageCountAsync(string queueName, HttpContext context);
    }

    public class QueueService(IFifoService fifoService,
        IXmlSerializer<MessageList> messageListSerializer,
        IXmlSerializer<QueueEnumerationResults> queueEnumerationResultsSerializer,
        ISettings settings) : IQueueService
    {
        public async Task<IActionResult> CreateQueueAsync(string queueName, HttpContext context)
        {
            Log.Information($"CreateQueue queueName = {queueName}");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            StatusCodeResult result = new(await fifoService.AddQueueAsync(queueName) ? 201 : 204);
            return result;
        }

        public async Task<IActionResult> GetQueuesAsync(CancellationToken? cancellationToken, HttpContext context)
        {
            Log.Information("GetQueuesAsync");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            if (!queries.TryGetValue("comp", out StringValues values) || !values.Contains("list"))
            {
                return new BadRequestResult();
            }
            QueueEnumerationResults results = new() { ServiceEndpoint = GetBaseUrl(context) };
            if (cancellationToken is { IsCancellationRequested: true }) return new StatusCodeResult(504);
            results.Queues.AddRange(await fifoService.GetQueuesAsync());
            if (cancellationToken is { IsCancellationRequested: true }) return new StatusCodeResult(504);
            results.MaxResults = 5000;
            return new ContentResult
            {
                Content = await queueEnumerationResultsSerializer.Serialize(results),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        public async Task<IActionResult> DeleteQueueAsync(string queueName, HttpContext context)
        {
            Log.Information($"DeleteQueueAsync name = {queueName}");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            await fifoService.DeleteQueueAsync(queueName);
            return new StatusCodeResult(204);
        }

        public async Task<IActionResult> GetMessagesAsync(string queueName, HttpContext context)
        {
            if (settings.QueueSettings.LogGetMessages) Log.Information($"GetMessagesAsync queueName = {queueName}");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            List<QueueMessage>? result = await fifoService.GetMessagesAsync(queueName,
                queries.TryGetValue("numofmessages", out StringValues numMessagesValue) ? Convert.ToInt32(numMessagesValue.First()) : null,
                queries.TryGetValue("peekonly", out StringValues peekOnlyValue) && Convert.ToBoolean(peekOnlyValue.First()));
            MessageList queueMessageList = new();
            if (result is not null) queueMessageList.QueueMessagesList.AddRange(result);
            return new ContentResult
            {
                Content = await messageListSerializer.Serialize(queueMessageList),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        public async Task<IActionResult> GetQueueMetadataAsync(string queueName, HttpContext context)
        {
            if (settings.QueueSettings.LogGetMessages) Log.Information($"GetMessagesAsync queueName = {queueName}");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            Models.Queue? result = await fifoService.GetQueueMetadataAsync(queueName);
            if (result is null) return new NotFoundResult();
            context.Response.Headers.Append("x-ms-approximate-messages-count", result.MessageCount.ToString());
            if (result.Metadata is null)
            {
                return new OkResult();
            }

            foreach (Metadata metadata in result.Metadata)
            {
                context.Response.Headers.Append($"x-ms-meta-{metadata.Key}", metadata.Value);
            }
            return new OkResult();
        }

        public async Task<IActionResult> PostMessageAsync(string queueName, QueueMessage message, int visibilityTimeout, int messageTtl, int timeout, HttpContext context)
        {
            Log.Information($"PostMessageAsync queueName = {queueName}, message={message.MessageText}, visibilityTimeout = {visibilityTimeout}, messageTtl = {messageTtl}, timeOut = {timeout}");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            MessageList queueMessageList = new();
            QueueMessage queueMessage = new();
            queueMessageList.QueueMessagesList.Add(queueMessage);
            queueMessage.DequeueCount = 0;
            queueMessage.ExpirationTime = DateTime.UtcNow.AddDays(7);
            queueMessage.InsertionTime = DateTime.UtcNow;
            queueMessage.MessageId = Guid.NewGuid();
            queueMessage.MessageText = message.MessageText;
            queueMessage.PopReceipt = Guid.NewGuid().ToString();
            queueMessage.VisibilityTimeout = visibilityTimeout;
            queueMessage.TimeNextVisible = DateTime.UtcNow;
            await fifoService.AddMessageAsync(queueName, queueMessage);
            return new ContentResult
            {
                Content = await messageListSerializer.Serialize(queueMessageList),
                ContentType = "application/xml",
                StatusCode = 201
            };
        }

        public async Task<IActionResult> DeleteMessageAsync(string queueName, Guid messageId, string popReceipt, HttpContext context)
        {
            Log.Information($"DeleteMessageAsync queueName = {queueName}, messageId = {messageId}, popReceipt = {popReceipt}");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            _ = await fifoService.DeleteMessageAsync(queueName, messageId, popReceipt);
            return new StatusCodeResult(204);
        }

        public async Task<IActionResult> ClearMessagesAsync(string queueName, HttpContext context)
        {
            Log.Information($"ClearMessagesAsync queueName = {queueName}");
            int result = await fifoService.ClearMessagesAsync(queueName);
            return new StatusCodeResult(result);
        }

        public async Task<IActionResult> MessageCountAsync(string queueName, HttpContext context)
        {
            Log.Information($"MessageCountAsync queueName = {queueName}");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            return new ContentResult
            {
                Content = (await fifoService.MessageCountAsync(queueName)).ToString(),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        private static Dictionary<string, StringValues> QueryProcessor(HttpRequest request)
        {
            return request
                .Query
                .Keys
                .ToDictionary(
                    key => key,
                    key => request
                        .Query
                        .TryGetValue(key, out StringValues values)
                        ? values
                        : []
                );
        }

        private static string GetBaseUrl(HttpContext context) => $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.Path.Value}";
    }
}