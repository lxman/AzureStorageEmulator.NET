using AzureStorageEmulator.NET.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AzureStorageEmulatorTests.Authentication
{
    public class QueueSharedKeyAuthenticatorTests
    {
        private readonly QueueSharedKeyAuthenticator _authenticator = new();
        private readonly Mock<HttpRequest> _requestMock = new();

        [Fact]
        public void Authenticate_ValidSharedKey_ReturnsTrue()
        {
            // Arrange
            _requestMock.Setup(r => r.Headers).Returns(new HeaderDictionary
            {
                ["Authorization"] = "SharedKey devstoreaccount1:Hos6sveA1DlQZyZNFGVGl6RRnDuuFlTzz6c9ewCDGlg=",
                ["Date"] = "Fri, 26 Nov 2021 00:00:00 GMT",
                ["Content-Type"] = "text/plain",
                ["Content-MD5"] = "Q2hlY2sgSW50ZWdyaXR5IQ==",
                ["x-ms-version"] = "2019-07-07"
            });
            _requestMock.Setup(r => r.Method).Returns("GET");
            _requestMock.Setup(r => r.Path).Returns("/devstoreaccount1/queue1");
            _requestMock.Setup(r => r.QueryString).Returns(new QueryString("?comp=metadata"));

            // Act
            bool result = _authenticator.Authenticate(_requestMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Authenticate_InvalidSharedKey_ReturnsFalse()
        {
            // Arrange
            _requestMock.Setup(r => r.Headers).Returns(new HeaderDictionary
            {
                ["Authorization"] = "SharedKey devstoreaccount1:InvalidKey"
            });
            _requestMock.Setup(r => r.Method).Returns("GET");
            _requestMock.Setup(r => r.Path).Returns("/devstoreaccount1/queue1");
            _requestMock.Setup(r => r.QueryString).Returns(new QueryString("?comp=metadata"));

            // Act
            bool result = _authenticator.Authenticate(_requestMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Authenticate_MissingAuthorizationHeader_ReturnsFalse()
        {
            // Arrange
            _requestMock.Setup(r => r.Headers).Returns(new HeaderDictionary());
            _requestMock.Setup(r => r.Method).Returns("GET");
            _requestMock.Setup(r => r.Path).Returns("/devstoreaccount1/queue1");
            _requestMock.Setup(r => r.QueryString).Returns(new QueryString("?comp=metadata"));

            // Act
            bool result = _authenticator.Authenticate(_requestMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Authenticate_InvalidAuthorizationType_ReturnsFalse()
        {
            // Arrange
            _requestMock.Setup(r => r.Headers).Returns(new HeaderDictionary
            {
                ["Authorization"] = "InvalidType devstoreaccount1:Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="
            });
            _requestMock.Setup(r => r.Method).Returns("GET");
            _requestMock.Setup(r => r.Path).Returns("/devstoreaccount1/queue1");
            _requestMock.Setup(r => r.QueryString).Returns(new QueryString("?comp=metadata"));

            // Act
            bool result = _authenticator.Authenticate(_requestMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Authenticate_MissingRequiredHeaders_ReturnsFalse()
        {
            // Arrange
            _requestMock.Setup(r => r.Headers).Returns(new HeaderDictionary
            {
                ["Authorization"] = "SharedKey devstoreaccount1:Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="
            });
            _requestMock.Setup(r => r.Method).Returns("GET");
            _requestMock.Setup(r => r.Path).Returns("/devstoreaccount1/queue1");
            _requestMock.Setup(r => r.QueryString).Returns(new QueryString(""));

            // Act
            bool result = _authenticator.Authenticate(_requestMock.Object);

            // Assert
            Assert.False(result);
        }
    }
}