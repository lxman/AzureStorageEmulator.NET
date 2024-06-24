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
        public Task<IActionResult> CreateQueueAsync(string queueName)
        {
            return queueService.CreateQueueAsync(queueName, HttpContext);
        }

        /// <summary>
        /// List the queues in the storage account.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ListQueuesAsync([FromQuery] int? timeout)
        {
            CancellationTokenSource? cancellationTokenSource = null;
            if (timeout.HasValue)
            {
                cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(timeout.Value * 1000);
            }
            return await queueService.ListQueuesAsync(HttpContext, cancellationTokenSource?.Token);
        }

        /// <summary>
        /// Delete a queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{queueName}")]
        public Task<IActionResult> DeleteQueueAsync(string queueName)
        {
            return queueService.DeleteQueueAsync(queueName, HttpContext);
        }

        /// <summary>
        /// Get all messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{queueName}")]
        public Task<IActionResult> GetQueueMetadataAsync(string queueName)
        {
            return queueService.GetQueueMetadataAsync(queueName, HttpContext);
        }

        #endregion QueueOps

        #region MessageOps

        /// <summary>
        /// Get 0 or more messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{queueName}/messages")]
        public Task<IActionResult> GetMessagesAsync(string queueName)
        {
            return queueService.GetMessagesAsync(queueName, HttpContext);
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
        public Task<IActionResult> PostMessageAsync(
            string queueName,
            [FromBody] QueueMessage message,
            [FromQuery] int visibilityTimeout = 0,
            [FromQuery] int messageTtl = 0,
            [FromQuery] int timeout = 0)
        {
            return queueService.PostMessageAsync(queueName, message, visibilityTimeout, messageTtl, timeout, HttpContext);
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
        public Task<IActionResult> DeleteMessageAsync(string queueName, Guid messageId, [FromQuery] string popReceipt)
        {
            return queueService.DeleteMessageAsync(queueName, messageId, popReceipt, HttpContext);
        }

        /// <summary>
        /// Delete all messages from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{queueName}/messages")]
        public Task<IActionResult> ClearMessagesAsync(string queueName)
        {
            return queueService.ClearMessagesAsync(queueName, HttpContext);
        }

        #endregion MessageOps
    }
}