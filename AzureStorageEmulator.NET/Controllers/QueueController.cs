using AzureStorageEmulator.NET.Queue;
using AzureStorageEmulator.NET.Queue.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Serilog;
using XmlTransformer.Queue.Models;

// ReSharper disable UnusedVariable
#pragma warning disable IDE0059

namespace AzureStorageEmulator.NET.Controllers
{
    [Route("devstoreaccount1")]
    [ApiController]
    [Host("*:10001")]
    public class QueueController(IMessageService messageService, IQueueSettings settings) : ControllerBase
    {
        /// <summary>
        /// Create a new queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns>201 if created, 204 if already exists</returns>
        [HttpPut]
        [Route("{queueName:alpha}")]
        public async Task<IActionResult> CreateQueue(string queueName)
        {
            Log.Information($"CreateQueue queueName = {queueName}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor();
            await Task.Delay(settings.Delay);
            return new StatusCodeResult(messageService.AddQueue(queueName) ? 201 : 204);
        }

        /// <summary>
        /// List the queues in the storage account.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ListQueues()
        {
            Log.Information("ListQueues");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor();
            return new ContentResult
            {
                Content = messageService.GetQueues(),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        /// <summary>
        /// Delete a queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{queueName:alpha}")]
        public IActionResult DeleteQueue(string queueName)
        {
            Log.Information($"DeleteQueue name = {queueName}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor();
            messageService.DeleteQueue(queueName);
            return new StatusCodeResult(204);
        }

        /// <summary>
        /// Get 0 or more messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="numOfMessages"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{queueName:alpha}/messages")]
        public async Task<IActionResult> GetMessages(string queueName, [FromQuery] int numOfMessages)
        {
            if (settings.LogGetMessages) Log.Information($"GetMessages queueName = {queueName}, numOfMessages = {numOfMessages}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor();
            await Task.Delay(settings.Delay);
            return new ContentResult
            {
                Content = messageService.GetMessages(queueName, numOfMessages),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        /// <summary>
        /// Get all messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{queueName:alpha}")]
        public async Task<IActionResult> GetAllMessages(string queueName)
        {
            if (settings.LogGetMessages) Log.Information($"GetMessages queueName = {queueName}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor();
            await Task.Delay(settings.Delay);
            return new ContentResult
            {
                Content = messageService.GetAllMessages(queueName),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        // TODO: Inspect query string usage here
        /// <summary>
        /// Put a message in the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="visibilityTimeout"></param>
        /// <param name="messageTtl"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{queueName:alpha}/messages")]
        public async Task<IActionResult> PostMessage(
            string queueName,
            [FromBody] QueueMessage message,
            [FromQuery] int visibilityTimeout = 0,
            [FromQuery] int messageTtl = 0,
            [FromQuery] int timeout = 0)
        {
            Log.Information($"PostMessage queueName = {queueName}, message={message.MessageText}, visibilityTimeout = {visibilityTimeout}, messageTtl = {messageTtl}, timeOut = {timeout}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor();
            await Task.Delay(settings.Delay);
            return new ContentResult
            {
                Content = messageService.AddMessage(queueName, message, visibilityTimeout, messageTtl, timeout),
                ContentType = "application/xml",
                StatusCode = 201
            };
        }

        /// <summary>
        /// Delete a message from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="messageId"></param>
        /// <param name="popReceipt"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{queueName:alpha}/messages/{messageId:guid}")]
        public async Task<IActionResult> DeleteMessage(string queueName, Guid messageId, [FromQuery] string popReceipt)
        {
            Log.Information($"DeleteMessage queueName = {queueName}, messageId = {messageId}, popReceipt = {popReceipt}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            Dictionary<string, StringValues> queries = QueryProcessor();
            _ = await messageService.DeleteMessage(queueName, messageId, popReceipt);
            await Task.Delay(settings.Delay);
            return new StatusCodeResult(204);
        }

        /// <summary>
        /// Delete all messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{queueName:alpha}/messages")]
        public IActionResult DeleteMessages(string queueName)
        {
            Log.Information($"DeleteMessages queueName = {queueName}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            messageService.DeleteMessages(queueName);
            return new StatusCodeResult(204);
        }

        private Dictionary<string, StringValues> QueryProcessor()
        {
            return messageService.QueryProcessor(Request);
        }
    }
}