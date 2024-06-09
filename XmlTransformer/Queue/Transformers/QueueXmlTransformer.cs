using System.Text;
using XmlTransformer.Queue.Models;

namespace XmlTransformer.Queue.Transformers
{
    public class QueueXmlTransformer : IXmlTransformer
    {
        public string ToXml(object o, bool getRequest)
        {
            switch (o)
            {
                case MessageList ml:
                    StringBuilder mlOut = new();
                    mlOut.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><QueueMessagesList>");
                    ml.QueueMessagesList.ForEach(message =>
                    {
                        mlOut.Append("<QueueMessage>");
                        mlOut.Append($"<MessageId>{message?.MessageId}</MessageId>");
                        mlOut.Append($"<InsertionTime>{message?.InsertionTime}</InsertionTime>");
                        mlOut.Append($"<ExpirationTime>{message?.ExpirationTime}</ExpirationTime>");
                        mlOut.Append($"<PopReceipt>{message?.PopReceipt}</PopReceipt>");
                        mlOut.Append($"<TimeNextVisible>{message?.TimeNextVisible}</TimeNextVisible>");
                        if (getRequest)
                        {
                            mlOut.Append($"<DequeueCount>{message?.DequeueCount}</DequeueCount>");
                            mlOut.Append($"<MessageText>{message?.MessageText}</MessageText>");
                        }
                        mlOut.Append("</QueueMessage>");
                    });
                    return mlOut.Append("</QueueMessagesList>").ToString();
                case List<string> queueList:
                    {
                        StringBuilder output = new();
                        output.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                        output.Append("<EnumerationResults ServiceEndpoint=\"http://127.0.0.1:10001/devstoreaccount1\">");
                        output.Append("<Prefix/>");
                        output.Append("<MaxResults>5000</MaxResults>");
                        output.Append("<Queues>");
                        queueList.ForEach(queueName => output.Append($"<Queue><Name>{queueName}</Name><Metadata/></Queue>"));
                        output.Append("</Queues>");
                        output.Append("<NextMarker/>");
                        output.Append("</EnumerationResults>");
                        return output.ToString();
                    }
                default:
                    return string.Empty;
            }
        }

        public object FromXml(string xml, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
