using AzureStorageEmulator.NET.Queue;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using XmlTransformer;
using XmlTransformer.Queue.Models;
using XmlTransformer.Queue.Transformers;
#pragma warning disable CA1859

namespace AzureStorageEmulator.NET.Controllers
{
    [Route("devstoreaccount1")]
    [ApiController]
    [Host("*:10001")]
    public class QueueController(IMessageService messageService, IQueueSettings settings) : ControllerBase
    {
        private readonly IXmlTransformer _transformer = new QueueXmlTransformer();

        /// <summary>
        /// Create a new queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns>201 if created, 204 if already exists</returns>
        [HttpPut]
        [Route("{queueName}")]
        public async Task<IActionResult> CreateQueue(string queueName)
        {
            Log.Information($"CreateQueue queueName = {queueName}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            await Task.Delay(settings.Delay);
            return new StatusCodeResult(messageService.AddQueue(queueName) ? 201 : 204);
        }

        /// <summary>
        /// List the queues in the storage account.
        /// </summary>
        /// <param name="comp"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ListQueues([FromQuery] string comp)
        {
            Log.Information($"ListQueues comp = {comp}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            if (comp != "list") return new StatusCodeResult(400);
            List<string> result = messageService.GetQueues();
            return new ContentResult
            {
                Content = _transformer.ToXml(result),
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
        [Route("{queueName}")]
        public IActionResult DeleteQueue(string queueName)
        {
            Log.Information($"DeleteQueue name = {queueName}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
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
        [Route("{queueName}/messages")]
        public async Task<IActionResult> GetMessages(string queueName, [FromQuery] int numOfMessages)
        {
            if (settings.LogGetMessages) Log.Information($"GetMessages queueName = {queueName}, numOfMessages = {numOfMessages}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            MessageList result = messageService.GetMessages(queueName, numOfMessages);
            await Task.Delay(settings.Delay);
            return new ContentResult
            {
                Content = _transformer.ToXml(result, true),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        /// <summary>
        /// Put a message in the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="visibilityTimeout"></param>
        /// <param name="messageTtl"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{queueName}/messages")]
        public async Task<IActionResult> PostMessage(
            string queueName,
            [FromBody] PostQueueMessage message,
            [FromQuery] int visibilityTimeout = 0,
            [FromQuery] int messageTtl = 0)
        {
            Log.Information($"PostMessage queueName = {queueName}, message={message.MessageText}, visibilityTimeout = {visibilityTimeout}, messageTtl = {messageTtl}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            MessageList msg = messageService.AddMessage(queueName, message, visibilityTimeout, messageTtl);
            await Task.Delay(settings.Delay);
            return new ContentResult
            {
                Content = _transformer.ToXml(msg),
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
        [Route("{queueName}/messages/{messageId:guid}")]
        public async Task<IActionResult> DeleteMessage(string queueName, Guid messageId, [FromQuery] string popReceipt)
        {
            Log.Information($"DeleteMessage queueName = {queueName}, messageId = {messageId}, popReceipt = {popReceipt}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
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
        [Route("{queueName}/messages")]
        public IActionResult DeleteMessages(string queueName)
        {
            Log.Information($"DeleteMessages queueName = {queueName}");
            if (!messageService.Authenticate(Request)) return new StatusCodeResult(403);
            messageService.DeleteMessages(queueName);
            return new StatusCodeResult(204);
        }
    }
}