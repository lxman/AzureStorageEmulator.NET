﻿using AzureStorageEmulator.NET.Authentication;
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
        Task<IActionResult> CreateQueue(string queueName, HttpContext context);

        Task<IActionResult> ListQueues(HttpContext context);

        Task<IActionResult> DeleteQueue(string queueName, HttpContext context);

        Task<IActionResult> PostMessage(string queueName, QueueMessage message, int visibilityTimeout, int messageTtl, int timeout, HttpContext context);

        Task<IActionResult> GetMessages(string queueName, HttpContext context);

        Task<IActionResult> GetQueueMetadata(string queueName, HttpContext context);

        Task<IActionResult> DeleteMessage(string queueName, Guid messageId, string popReceipt, HttpContext context);

        Task<IActionResult> DeleteMessages(string queueName, HttpContext context);

        Task<IActionResult> MessageCount(string queueName, HttpContext context);
    }

    public class QueueService(IFifoService fifoService,
        IAuthenticator authenticator,
        IXmlSerializer<MessageList> messageListSerializer,
        IXmlSerializer<QueueEnumerationResults> queueEnumerationResultsSerializer,
        IQueueSettings settings) : IQueueService
    {
        public async Task<IActionResult> CreateQueue(string queueName, HttpContext context)
        {
            Log.Information($"CreateQueue queueName = {queueName}");
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            StatusCodeResult result = new(await fifoService.AddQueue(queueName) ? 201 : 204);
            SetResponseHeaders(context);
            return result;
        }

        public async Task<IActionResult> ListQueues(HttpContext context)
        {
            Log.Information("ListQueues");
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            if (!queries.TryGetValue("comp", out StringValues values) || !values.Contains("list"))
            {
                return new BadRequestResult();
            }
            QueueEnumerationResults results = new();
            results.Queues.AddRange(await fifoService.GetQueues());
            results.MaxResults = 5000;
            SetResponseHeaders(context);
            return new ContentResult
            {
                Content = queueEnumerationResultsSerializer.Serialize(results),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        public async Task<IActionResult> DeleteQueue(string queueName, HttpContext context)
        {
            Log.Information($"DeleteQueueAsync name = {queueName}");
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            await fifoService.DeleteQueueAsync(queueName);
            return new StatusCodeResult(204);
        }

        public async Task<IActionResult> GetMessages(string queueName, HttpContext context)
        {
            if (settings.LogGetMessages) Log.Information($"GetMessagesAsync queueName = {queueName}");
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            List<QueueMessage>? result = await fifoService.GetMessagesAsync(queueName,
                queries.TryGetValue("numofmessages", out StringValues numMessagesValue) ? Convert.ToInt32(numMessagesValue.First()) : null,
                queries.TryGetValue("peekonly", out StringValues peekOnlyValue) && Convert.ToBoolean(peekOnlyValue.First()));
            MessageList queueMessageList = new();
            if (result is not null) queueMessageList.QueueMessagesList.AddRange(result);
            return new ContentResult
            {
                Content = messageListSerializer.Serialize(queueMessageList),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        public async Task<IActionResult> GetQueueMetadata(string queueName, HttpContext context)
        {
            if (settings.LogGetMessages) Log.Information($"GetMessagesAsync queueName = {queueName}");
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            SetResponseHeaders(context);
            Models.Queue? result = await fifoService.GetQueueMetadata(queueName);
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

        public async Task<IActionResult> PostMessage(string queueName, QueueMessage message, int visibilityTimeout, int messageTtl, int timeout, HttpContext context)
        {
            Log.Information($"PostMessage queueName = {queueName}, message={message.MessageText}, visibilityTimeout = {visibilityTimeout}, messageTtl = {messageTtl}, timeOut = {timeout}");
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
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
                Content = messageListSerializer.Serialize(queueMessageList),
                ContentType = "application/xml",
                StatusCode = 201
            };
        }

        public async Task<IActionResult> DeleteMessage(string queueName, Guid messageId, string popReceipt, HttpContext context)
        {
            Log.Information($"DeleteMessageAsync queueName = {queueName}, messageId = {messageId}, popReceipt = {popReceipt}");
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            _ = await fifoService.DeleteMessageAsync(queueName, messageId, popReceipt);
            return new StatusCodeResult(204);
        }

        public async Task<IActionResult> DeleteMessages(string queueName, HttpContext context)
        {
            Log.Information($"DeleteMessages queueName = {queueName}");
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
            await fifoService.DeleteMessages(queueName);
            return new StatusCodeResult(204);
        }

        public async Task<IActionResult> MessageCount(string queueName, HttpContext context)
        {
            Log.Information($"MessageCountAsync queueName = {queueName}");
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
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

        private bool Authenticate(HttpRequest request) => authenticator.Authenticate(request);

        private static void SetResponseHeaders(HttpContext context)
        {
            context.Response.Headers.Append("x-ms-version", "2023-11-03");
            context.Response.Headers.Append("x-ms-request-id", Guid.NewGuid().ToString());
            string? clientRequestId = context.Request.Headers["x-ms-client-request-id"][0];
            if (clientRequestId is not null && clientRequestId.Length <= 1024)
                context.Response.Headers.Append("x-ms-client-request-id", clientRequestId);
        }
    }
}