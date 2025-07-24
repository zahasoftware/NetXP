using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NetXP.IAs.Chats.Ollama;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Moq.Protected;

namespace NetXP.UnitTest.IAs.Chats.Ollamas
{
    [TestClass]
    public class OllamaChatServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private Mock<IOptions<AIChatConfig>> _configMock;
        private AIChatConfig _config;
        private OllamaChatService _service;

        [TestInitialize]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _config = new AIChatConfig
            {
                BaseUrl = "http://test.com",
                GenerateEndpoint = "generate",
                ModelsEndpoint = "models"
            };
            _configMock = new Mock<IOptions<AIChatConfig>>();
            _configMock.Setup(c => c.Value).Returns(_config);
            _service = new OllamaChatService(_httpClient, _configMock.Object);
        }

        [TestMethod]
        public async Task GenerateResponseAsync_ShouldReturnResponse()
        {
            // Arrange
            var prompt = "Hello";
            var responseContent = new OllamaChatService.OllamaResponse { Response = "Hi there!" };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(responseContent)
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _service.GenerateResponseAsync(prompt);

            // Assert
            Assert.AreEqual("Hi there!", result);
        }

        [TestMethod]
        public async Task GetAvailableModelsAsync_ShouldReturnModels()
        {
            // Arrange
            var models = new List<string> { "model1", "model2" };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(models)
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _service.GetAvailableModelsAsync();

            // Assert
            CollectionAssert.AreEqual(models, (System.Collections.ICollection)result);
        }

        [TestMethod]
        public async Task SetModelAsync_ShouldSetModelId()
        {
            // Arrange
            var modelId = "model1";

            // Act
            await _service.SetModelAsync(modelId);

            // Assert
            // Use reflection to check the private field _currentModelId
            var field = typeof(OllamaChatService).GetField("_currentModelId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = field.GetValue(_service);
            Assert.AreEqual(modelId, value);
        }
    }
}
