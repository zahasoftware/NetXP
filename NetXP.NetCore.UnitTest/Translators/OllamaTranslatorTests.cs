using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using NetXP.Translators.OllamaTranslator;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NetXP.UnitTest.Translators
{
    [TestClass]
    public class OllamaTranslatorTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private Mock<IOptions<OllamaTranslatorOptions>> _optionsMock;
        private OllamaTranslatorOptions _options;
        private OllamaTranslatorImplementation _translator;

        [TestInitialize]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            
            _options = new OllamaTranslatorOptions
            {
                BaseUrl = "http://localhost:11434/api",
                GenerateEndpoint = "generate",
                DefaultModel = "llama3.2"
            };
            
            _optionsMock = new Mock<IOptions<OllamaTranslatorOptions>>();
            _optionsMock.Setup(o => o.Value).Returns(_options);
            
            _translator = new OllamaTranslatorImplementation(_optionsMock.Object, _httpClient);
        }

        [TestMethod]
        public async Task TranslateTextAsync_WithoutInstruction_ShouldReturnTranslatedText()
        {
            // Arrange
            var textToTranslate = "Hello, how are you?";
            var toLanguage = "es-ES";
            var expectedTranslation = "Hola, ¿cómo estás?";

            var ollamaResponse = new { response = expectedTranslation };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(ollamaResponse))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _translator.TranslateTextAsync(textToTranslate, toLanguage);

            // Assert
            Assert.AreEqual(expectedTranslation, result);
        }

        [TestMethod]
        public async Task TranslateTextAsync_WithInstruction_ShouldReturnTranslatedText()
        {
            // Arrange
            var textToTranslate = "Hello, how are you?";
            var toLanguage = "es-ES";
            var instruction = "Translate the following text to Spanish using formal language.";
            var expectedTranslation = "Hola, ¿cómo está usted?";

            var ollamaResponse = new { response = expectedTranslation };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(ollamaResponse))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _translator.TranslateTextAsync(textToTranslate, toLanguage, instruction);

            // Assert
            Assert.AreEqual(expectedTranslation, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithEmptyBaseUrl_ShouldThrowException()
        {
            // Arrange
            var invalidOptions = new OllamaTranslatorOptions
            {
                BaseUrl = "",
                GenerateEndpoint = "generate",
                DefaultModel = "llama3.2"
            };
            
            var optionsMock = new Mock<IOptions<OllamaTranslatorOptions>>();
            optionsMock.Setup(o => o.Value).Returns(invalidOptions);

            // Act
            _ = new OllamaTranslatorImplementation(optionsMock.Object, _httpClient);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithEmptyDefaultModel_ShouldThrowException()
        {
            // Arrange
            var invalidOptions = new OllamaTranslatorOptions
            {
                BaseUrl = "http://localhost:11434/api",
                GenerateEndpoint = "generate",
                DefaultModel = ""
            };
            
            var optionsMock = new Mock<IOptions<OllamaTranslatorOptions>>();
            optionsMock.Setup(o => o.Value).Returns(invalidOptions);

            // Act
            _ = new OllamaTranslatorImplementation(optionsMock.Object, _httpClient);

            // Assert - Exception expected
        }

        [TestMethod]
        public async Task TranslateTextAsync_WhenApiReturnsError_ShouldThrowException()
        {
            // Arrange
            var textToTranslate = "Hello";
            var toLanguage = "es-ES";

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                async () => await _translator.TranslateTextAsync(textToTranslate, toLanguage));
        }
    }
}
