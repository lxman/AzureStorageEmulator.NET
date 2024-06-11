﻿using AzureStorageEmulator.NET.Authentication;
using AzureStorageEmulator.NET.Queue.Services;
using AzureStorageEmulator.NET.XmlSerialization.Queue;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AzureStorageEmulatorTests.Queue.Services
{
    public class MessageServiceTests
    {
        private const string QueueName = "testQueue";
        private static readonly Mock<IFifoService> MockFifoService = new();
        private static readonly Mock<IAuthenticator> MockAuthenticator = new();
        private static readonly EnumerationResultsSerializer QueueSerializer = new();
        private static readonly MessageListSerializer MessageListSerializer = new();
        private readonly MessageService _messageService = new(MockFifoService.Object, MockAuthenticator.Object, QueueSerializer, MessageListSerializer);

        [Fact]
        public void AddQueue_ShouldAddQueueSuccessfully()
        {
            MockFifoService.Setup(service => service.AddQueue(It.IsAny<string>())).Returns(true);

            bool result = _messageService.AddQueue(QueueName);

            Assert.True(result);
            MockFifoService.Verify(service => service.AddQueue(QueueName), Times.Once);
        }

        [Fact]
        public void DeleteQueue_ShouldRemoveQueueSuccessfully()
        {
            _messageService.DeleteQueue(QueueName);

            MockFifoService.Verify(service => service.DeleteQueue(QueueName), Times.Once);
        }

        // TODO: Fix this test
        //[Fact]
        //public void AddMessage_ShouldAddMessageToQueue()
        //{
        //    EnumerationResults message = new() { MessageText = "testMessage" };

        //    string result = _messageService.AddMessage(QueueName, message, 0, 0);

        //    //Assert.Single(result.QueueMessagesList);
        //    MockFifoService.Verify(service => service.AddMessage(QueueName, It.IsAny<QueueMessage>()), Times.Once);
        //}

        [Fact]
        public async Task DeleteMessage_ShouldDeleteMessageFromQueue()
        {
            Guid messageId = Guid.NewGuid();
            string popReceipt = Guid.NewGuid().ToString();

            await _messageService.DeleteMessage(QueueName, messageId, popReceipt);

            MockFifoService.Verify(service => service.DeleteMessage(QueueName, messageId, popReceipt), Times.Once);
        }

        [Fact]
        public void GetQueues_ShouldReturnListOfQueues()
        {
            List<XmlTransformer.Queue.Models.Queue> queues =
                [
                    new XmlTransformer.Queue.Models.Queue { Name = "queue1" },
                    new XmlTransformer.Queue.Models.Queue { Name = "queue2" },
                    new XmlTransformer.Queue.Models.Queue { Name = "queue3" }
                ];
            MockFifoService.Setup(service => service.GetQueues()).Returns(queues);
            MockAuthenticator.Setup(a => a.Authenticate(It.IsAny<HttpRequest>())).Returns(true);

            string result = _messageService.GetQueues();

            Assert.Equal(
                "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><EnumerationResults ServiceEndpoint=\"http://127.0.0.1:10001/devstoreaccount1\"><Prefix /><MaxResults>5000</MaxResults><Queues><Queue><Name>queue1</Name><Metadata /></Queue><Queue><Name>queue2</Name><Metadata /></Queue><Queue><Name>queue3</Name><Metadata /></Queue></Queues><NextMarker /></EnumerationResults>",
                result);
            MockFifoService.Verify(service => service.GetQueues(), Times.Once);
        }

        // TODO: Fix this test
        //[Fact]
        //public void GetMessages_ShouldReturnListOfMessages()
        //{
        //    List<QueueMessage?> messages = [new QueueMessage(), new QueueMessage()];
        //    MockFifoService.Setup(service => service.GetMessages(QueueName, It.IsAny<int>())).Returns(messages);

        //    string result = _messageService.GetMessages(QueueName, 2);

        //    //Assert.Equal(2, result.QueueMessagesList.Count);
        //    MockFifoService.Verify(service => service.GetMessages(QueueName, 2), Times.Once);
        //}
    }
}