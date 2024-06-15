using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageEmulator.NET.Controllers
{
    [Route("devstoreaccount1")]
    [ApiController]
    [Host("*:10001")]
    public class QueueController(IQueueService queueService) : ControllerBase
    {
        // TODO: Implement a mechanism to return 504 Gateway Timeout if processing takes too long

        #region QueueOps

        /// <summary>
        /// Create a new queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns>201 if created, 204 if already exists</returns>
        [HttpPut]
        [Route("{queueName}")]
        public Task<IActionResult> CreateQueue(string queueName)
        {
            return queueService.CreateQueue(queueName, HttpContext);
        }

        /// <summary>
        /// List the queues in the storage account.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Task<IActionResult> ListQueues()
        {
            return queueService.ListQueues(HttpContext);
        }

        /// <summary>
        /// Delete a queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{queueName}")]
        public Task<IActionResult> DeleteQueue(string queueName)
        {
            return queueService.DeleteQueue(queueName, HttpContext);
        }

        /// <summary>
        /// Get all messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{queueName}")]
        public Task<IActionResult> GetQueueMetadata(string queueName)
        {
            return queueService.GetQueueMetadata(queueName, HttpContext);
        }

        #endregion QueueOps

        #region MessageOps

        /// <summary>
        /// Get 0 or more messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="numOfMessages"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{queueName}/messages")]
        public Task<IActionResult> GetMessages(string queueName)
        {
            return queueService.GetMessages(queueName, HttpContext);
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
        [Route("{queueName}/messages")]
        public Task<IActionResult> PostMessage(
            string queueName,
            [FromBody] QueueMessage message,
            [FromQuery] int visibilityTimeout = 0,
            [FromQuery] int messageTtl = 0,
            [FromQuery] int timeout = 0)
        {
            return queueService.PostMessage(queueName, message, visibilityTimeout, messageTtl, timeout, HttpContext);
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
        public Task<IActionResult> DeleteMessage(string queueName, Guid messageId, [FromQuery] string popReceipt)
        {
            return queueService.DeleteMessage(queueName, messageId, popReceipt, HttpContext);
        }

        /// <summary>
        /// Delete all messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{queueName}/messages")]
        public Task<IActionResult> DeleteMessages(string queueName)
        {
            return queueService.DeleteMessages(queueName, HttpContext);
        }

        #endregion MessageOps
    }
}