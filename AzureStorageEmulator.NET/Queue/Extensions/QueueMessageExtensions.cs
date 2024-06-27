using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Models.MessageResponses;

namespace AzureStorageEmulator.NET.Queue.Extensions
{
    public static class QueueMessageExtensions
    {
        public static PutMessageResponse ToPutMessageResponse(this QueueMessage message)
        {
            return new PutMessageResponse
            {
                MessageId = message.MessageId,
                PopReceipt = message.PopReceipt,
                InsertionTime = message.InsertionTime,
                ExpirationTime = message.ExpirationTime,
                TimeNextVisible = message.TimeNextVisible
            };
        }

        public static List<PeekMessageResponse> ToPeekMessageResponseList(this List<QueueMessage> messages)
        {
            return messages.Select(ToPeekMessageResponse).ToList();
        }

        public static List<GetMessageResponse> ToGetMessageResponseList(this List<QueueMessage> messages)
        {
            return messages.Select(ToGetMessageResponse).ToList();
        }

        private static PeekMessageResponse ToPeekMessageResponse(this QueueMessage message)
        {
            return new PeekMessageResponse
            {
                MessageId = message.MessageId,
                MessageText = message.MessageText,
                InsertionTime = message.InsertionTime,
                ExpirationTime = message.ExpirationTime,
                DequeueCount = message.DequeueCount
            };
        }

        private static GetMessageResponse ToGetMessageResponse(this QueueMessage message)
        {
            return new GetMessageResponse
            {
                MessageId = message.MessageId,
                MessageText = message.MessageText,
                InsertionTime = message.InsertionTime,
                ExpirationTime = message.ExpirationTime,
                PopReceipt = message.PopReceipt,
                TimeNextVisible = message.TimeNextVisible,
                DequeueCount = message.DequeueCount
            };
        }
    }
}
