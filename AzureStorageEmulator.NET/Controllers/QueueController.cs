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
        #region QueueOps

        /// <summary>
        /// Create a new queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="timeout"></param>
        /// <returns>201 if created, 204 if already exists</returns>
        [HttpPut]
        [Route("{queueName}")]
        public Task<IActionResult> CreateQueueAsync(string queueName, [FromQuery] int timeout = 0)
        {
            return queueService.CreateQueueAsync(queueName, timeout, HttpContext);
        }

        /// <summary>
        /// List the queues in the storage account.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ListQueuesAsync([FromQuery] int timeout = 0)
        {
            return await queueService.ListQueuesAsync(timeout, HttpContext);
        }

        /// <summary>
        /// Get all messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{queueName}")]
        public Task<IActionResult> GetQueueMetadataAsync(string queueName, [FromQuery] int timeout = 0)
        {
            return queueService.GetQueueMetadataAsync(queueName, timeout, HttpContext);
        }

        /// <summary>
        /// Delete a queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{queueName}")]
        public Task<IActionResult> DeleteQueueAsync(string queueName, [FromQuery] int timeout = 0)
        {
            return queueService.DeleteQueueAsync(queueName, timeout, HttpContext);
        }

        #endregion QueueOps

        #region MessageOps

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
        public Task<IActionResult> PutMessageAsync(
            string queueName,
            [FromBody] QueueMessage message,
            [FromQuery] int visibilityTimeout = 0,
            [FromQuery] int messageTtl = 0,
            [FromQuery] int timeout = 0)
        {
            return queueService.PutMessageAsync(queueName, message, visibilityTimeout, messageTtl, timeout, HttpContext);
        }

        /// <summary>
        /// Get 0 or more messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{queueName}/messages")]
        public Task<IActionResult> GetMessagesAsync(string queueName, [FromQuery] int timeout = 0)
        {
            return queueService.GetMessagesAsync(queueName, timeout, HttpContext);
        }

        /// <summary>
        /// Delete a message from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="messageId"></param>
        /// <param name="popReceipt"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{queueName}/messages/{messageId:guid}")]
        public Task<IActionResult> DeleteMessageAsync(
            string queueName,
            Guid messageId,
            [FromQuery] string popReceipt,
            [FromQuery] int timeout = 0)
        {
            return queueService.DeleteMessageAsync(queueName, messageId, popReceipt, 0, HttpContext);
        }

        /// <summary>
        /// Delete all messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{queueName}/messages")]
        public Task<IActionResult> ClearMessagesAsync(string queueName, [FromQuery] int timeout = 0)
        {
            return queueService.ClearMessagesAsync(queueName, timeout, HttpContext);
        }

        #endregion MessageOps
    }
}