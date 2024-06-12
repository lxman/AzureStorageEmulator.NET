using AzureStorageEmulator.NET.Authentication;
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
        IActionResult ListQueues(HttpRequest request);

        Task<IActionResult> CreateQueue(string queueName, HttpRequest request);

        IActionResult DeleteQueue(string queueName, HttpRequest request);

        Task<IActionResult> PostMessage(string queueName, QueueMessage message, int visibilityTimeout, int messageTtl, int timeout, HttpRequest request);

        Task<IActionResult> GetMessages(string queueName, int numOfMessages, HttpRequest request);

        Task<IActionResult> GetAllMessages(string queueName, HttpRequest request);

        Task<IActionResult> DeleteMessage(string queueName, Guid messageId, string popReceipt, HttpRequest request);

        IActionResult DeleteMessages(string queueName, HttpRequest request);

        IActionResult MessageCount(string queueName, HttpRequest request);
    }

    public class QueueService(IFifoService fifoService,
        IAuthenticator authenticator,
        IXmlSerializer<EnumerationResults> enumerationResultsSerializer,
        IXmlSerializer<MessageList> messageListSerializer,
        IQueueSettings settings) : IQueueService
    {
        public bool Authenticate(HttpRequest request)
        {
            return authenticator.Authenticate(request);
        }

        public IActionResult ListQueues(HttpRequest request)
        {
            Log.Information("ListQueues");
            if (!Authenticate(request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(request);
            if (!queries.TryGetValue("comp", out StringValues values) || !values.Contains("list"))
            {
                return new BadRequestResult();
            }
            EnumerationResults results = new();
            results.Queues.AddRange(fifoService.GetQueues());
            results.MaxResults = 5000;
            return new ContentResult
            {
                Content = enumerationResultsSerializer.Serialize(results),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        public async Task<IActionResult> GetMessages(string queueName, int numOfMessages, HttpRequest request)
        {
            if (settings.LogGetMessages) Log.Information($"GetMessages queueName = {queueName}, numOfMessages = {numOfMessages}");
            if (!Authenticate(request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(request);
            if (numOfMessages == 0) numOfMessages = 1;
            List<QueueMessage?>? result = fifoService.GetMessages(queueName, numOfMessages);
            MessageList queueMessageList = new();
            if (result is not null) queueMessageList.QueueMessagesList.AddRange(result);
            await Task.Delay(settings.Delay);
            return new ContentResult
            {
                Content = messageListSerializer.Serialize(queueMessageList),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        public async Task<IActionResult> GetAllMessages(string queueName, HttpRequest request)
        {
            if (settings.LogGetMessages) Log.Information($"GetMessages queueName = {queueName}");
            if (!Authenticate(request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(request);
            List<QueueMessage?>? result = fifoService.GetAllMessages(queueName);
            MessageList queueMessageList = new();
            if (result is not null) queueMessageList.QueueMessagesList.AddRange(result);
            await Task.Delay(settings.Delay);
            return new ContentResult
            {
                Content = messageListSerializer.Serialize(queueMessageList),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        public async Task<IActionResult> PostMessage(string queueName, QueueMessage message, int visibilityTimeout, int messageTtl, int timeout, HttpRequest request)
        {
            Log.Information($"PostMessage queueName = {queueName}, message={message.MessageText}, visibilityTimeout = {visibilityTimeout}, messageTtl = {messageTtl}, timeOut = {timeout}");
            if (!Authenticate(request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(request);
            MessageList queueMessageList = new();
            QueueMessage queueMessage = new();
            queueMessageList.QueueMessagesList.Add(queueMessage);
            queueMessage.DequeueCount = 0;
            queueMessage.ExpirationTime = DateTime.UtcNow.AddDays(7);
            queueMessage.InsertionTime = DateTime.UtcNow;
            queueMessage.MessageId = Guid.NewGuid();
            queueMessage.MessageText = message.MessageText;
            queueMessage.PopReceipt = Guid.NewGuid().ToString();
            queueMessage.TimeNextVisible = DateTime.UtcNow.AddSeconds(visibilityTimeout);
            fifoService.AddMessage(queueName, queueMessage);
            await Task.Delay(settings.Delay);
            return new ContentResult
            {
                Content = messageListSerializer.Serialize(queueMessageList),
                ContentType = "application/xml",
                StatusCode = 201
            };
        }

        public async Task<IActionResult> CreateQueue(string queueName, HttpRequest request)
        {
            Log.Information($"CreateQueue queueName = {queueName}");
            if (!Authenticate(request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(request);
            StatusCodeResult result = new StatusCodeResult(fifoService.AddQueue(queueName) ? 201 : 204);
            await Task.Delay(settings.Delay);
            return result;
        }

        public IActionResult DeleteQueue(string queueName, HttpRequest request)
        {
            Log.Information($"DeleteQueue name = {queueName}");
            if (!Authenticate(request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(request);
            fifoService.DeleteQueue(queueName);
            return new StatusCodeResult(204);
        }

        public async Task<IActionResult> DeleteMessage(string queueName, Guid messageId, string popReceipt, HttpRequest request)
        {
            Log.Information($"DeleteMessage queueName = {queueName}, messageId = {messageId}, popReceipt = {popReceipt}");
            if (!Authenticate(request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(request);
            _ = fifoService.DeleteMessage(queueName, messageId, popReceipt);
            await Task.Delay(settings.Delay);
            return new StatusCodeResult(204);
        }

        public IActionResult DeleteMessages(string queueName, HttpRequest request)
        {
            Log.Information($"DeleteMessages queueName = {queueName}");
            if (!Authenticate(request)) return new StatusCodeResult(403);
            fifoService.DeleteMessages(queueName);
            return new StatusCodeResult(204);
        }

        public IActionResult MessageCount(string queueName, HttpRequest request)
        {
            Log.Information($"MessageCount queueName = {queueName}");
            if (!Authenticate(request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(request);
            return new ContentResult
            {
                Content = fifoService.MessageCount(queueName).ToString(),
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
    }
}