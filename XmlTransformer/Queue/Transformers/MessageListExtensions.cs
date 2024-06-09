using System.Text;
using XmlTransformer.Queue.Models;

namespace XmlTransformer.Queue.Transformers
{
    public static class MessageListExtensions
    {
        public static string ToXml(this MessageList messageList, bool getRequest = false)
        {
            StringBuilder output = new();
            output.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><QueueMessagesList>");
            messageList.QueueMessagesList.ForEach(message =>
            {
                output.Append("<QueueMessage>");
                output.Append($"<MessageId>{message?.MessageId}</MessageId>");
                output.Append($"<InsertionTime>{message?.InsertionTime}</InsertionTime>");
                output.Append($"<ExpirationTime>{message?.ExpirationTime}</ExpirationTime>");
                output.Append($"<PopReceipt>{message?.PopReceipt}</PopReceipt>");
                output.Append($"<TimeNextVisible>{message?.TimeNextVisible}</TimeNextVisible>");
                if (getRequest)
                {
                    output.Append($"<DequeueCount>{message?.DequeueCount}</DequeueCount>");
                    output.Append($"<MessageText>{message?.MessageText}</MessageText>");
                }
                output.Append("</QueueMessage>");
            });
            return output.Append("</QueueMessagesList>").ToString();
        }
    }
}