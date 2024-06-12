using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageEmulator.NET.Controllers
{
    [Route("devstoreaccount1")]
    [ApiController]
    [Host("*:10001")]
    public class QueueController(IMessageService messageService) : ControllerBase
    {
        /// <summary>
        /// Create a new queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns>201 if created, 204 if already exists</returns>
        [HttpPut]
        [Route("{queueName:alpha}")]
        public Task<IActionResult> CreateQueue(string queueName)
        {
            return messageService.CreateQueue(queueName, Request);
        }

        /// <summary>
        /// List the queues in the storage account.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ListQueues()
        {
            return messageService.ListQueues(Request);
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
            return messageService.DeleteQueue(queueName, Request);
        }

        /// <summary>
        /// Get 0 or more messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="numOfMessages"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{queueName:alpha}/messages")]
        public Task<IActionResult> GetMessages(string queueName, [FromQuery] int numOfMessages)
        {
            return messageService.GetMessages(queueName, numOfMessages, Request);
        }

        /// <summary>
        /// Get all messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{queueName:alpha}")]
        public Task<IActionResult> GetAllMessages(string queueName)
        {
            return messageService.GetAllMessages(queueName, Request);
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
        public Task<IActionResult> PostMessage(
            string queueName,
            [FromBody] QueueMessage message,
            [FromQuery] int visibilityTimeout = 0,
            [FromQuery] int messageTtl = 0,
            [FromQuery] int timeout = 0)
        {
            return messageService.PostMessage(queueName, message, visibilityTimeout, messageTtl, timeout, Request);
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
        public Task<IActionResult> DeleteMessage(string queueName, Guid messageId, [FromQuery] string popReceipt)
        {
            return messageService.DeleteMessage(queueName, messageId, popReceipt, Request);
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
            return messageService.DeleteMessages(queueName, Request);
        }
    }
}