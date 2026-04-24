using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetXP.IAs.ImageGeneratorAI;
using NetXP.ImageGeneratorAI.ComfyBridgeApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetXP.NetCoreUnitTest.ImageGeneratorAI
{
    [TestClass]
    public class ComfyBridgeClientTests
    {
        [TestMethod]
        public async Task Generate_MapsInputsUsingTemplateInputs()
        {
            var handler = new QueueMessageHandler();
            handler.EnqueueJson(HttpStatusCode.OK,
                """
                {
                  "templates": [
                    {
                      "name": "text2image",
                      "version": "1.0",
                      "category": "image",
                      "inputs": {
                        "prompt": { "type": "string" },
                        "negativePrompt": { "type": "string" },
                        "width": { "type": "number" },
                        "height": { "type": "number" },
                        "batch_size": { "type": "number" },
                        "checkpointName": { "type": "string" },
                        "seed": { "type": "number" }
                      }
                    }
                  ]
                }
                """);
            handler.EnqueueJson(HttpStatusCode.OK, "{ \"jobId\": \"job-123\", \"status\": \"Queued\" }");

            var client = CreateClient(handler);

            var result = await client.Generate(new OptionsImageGenerator
            {
                Prompt = "A cyberpunk city",
                NegativePrompt = "blurry",
                Width = 1024,
                Height = 768,
                NumImages = 2,
                ModelId = "model-v1",
                ExtraOptions = new { seed = 42, ignored = "x" }
            });

            Assert.AreEqual("job-123", result.Id);

            var postRequest = handler.CapturedRequests.Single(r =>
                r.Method == HttpMethod.Post && r.Path == "/api/v1/image/text2image");

            using var payloadDocument = JsonDocument.Parse(postRequest.Body ?? "{}");
            var payloadRoot = payloadDocument.RootElement;

            Assert.AreEqual("A cyberpunk city", payloadRoot.GetProperty("prompt").GetString());
            Assert.AreEqual("blurry", payloadRoot.GetProperty("negativePrompt").GetString());
            Assert.AreEqual(1024, payloadRoot.GetProperty("width").GetInt32());
            Assert.AreEqual(768, payloadRoot.GetProperty("height").GetInt32());
            Assert.AreEqual(2, payloadRoot.GetProperty("batch_size").GetInt32());
            Assert.AreEqual("model-v1", payloadRoot.GetProperty("checkpointName").GetString());
            Assert.AreEqual(42, payloadRoot.GetProperty("seed").GetInt32());
            Assert.IsFalse(payloadRoot.TryGetProperty("ignored", out _));
        }

        [TestMethod]
        public async Task Generate_MapsConfiguredExtraOptionsInputs()
        {
            var handler = new QueueMessageHandler();
            handler.EnqueueJson(HttpStatusCode.OK,
                """
                {
                  "templates": [
                    {
                      "name": "text2image",
                      "version": "1.0",
                      "category": "image",
                      "inputs": {
                        "prompt": { "type": "string" },
                        "seed": { "type": "number" },
                        "cfg": { "type": "number" }
                      }
                    }
                  ]
                }
                """);
            handler.EnqueueJson(HttpStatusCode.OK, "{ \"jobId\": \"job-456\", \"status\": \"Queued\" }");

            var client = CreateClient(handler, o =>
            {
                o.ExtraOptions = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["seed"] = 41,
                    ["cfg"] = 7
                };
            });

            var result = await client.Generate(new OptionsImageGenerator
            {
                Prompt = "A futuristic robot",
                Width = 1024,
                Height = 768,
                NumImages = 1
            });

            Assert.AreEqual("job-456", result.Id);

            var postRequest = handler.CapturedRequests.Single(r =>
                r.Method == HttpMethod.Post && r.Path == "/api/v1/image/text2image");

            using var payloadDocument = JsonDocument.Parse(postRequest.Body ?? "{}");
            var payloadRoot = payloadDocument.RootElement;

            Assert.AreEqual(41, payloadRoot.GetProperty("seed").GetInt32());
            Assert.AreEqual(7, payloadRoot.GetProperty("cfg").GetInt32());
        }

        [TestMethod]
        public async Task GetImages_PollsUntilCompleted_AndDownloadsOutputs()
        {
            var handler = new QueueMessageHandler();
            handler.EnqueueJson(HttpStatusCode.OK, "{ \"status\": \"Running\" }");
            handler.EnqueueJson(HttpStatusCode.OK,
                """
                {
                  "status": "Completed",
                  "assetUrls": [
                    "https://cdn.example/output-0.png",
                    "https://cdn.example/output-1.png"
                  ]
                }
                """);
            handler.EnqueueBinary(HttpStatusCode.OK, [1, 2, 3]);
            handler.EnqueueBinary(HttpStatusCode.OK, [4, 5, 6]);

            var client = CreateClient(handler);

            var result = await client.GetImages(new ResultGenerate { Id = "job-88" });

            Assert.AreEqual(2, result.Images.Count);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, result.Images[0].Image);
            CollectionAssert.AreEqual(new byte[] { 4, 5, 6 }, result.Images[1].Image);
            Assert.AreEqual("https://cdn.example/output-0.png", result.Images[0].Url);
            Assert.AreEqual("https://cdn.example/output-1.png", result.Images[1].Url);

            Assert.AreEqual(2, handler.CapturedRequests.Count(r =>
                r.Method == HttpMethod.Get && r.Path == "/api/v1/jobs/job-88"));
            Assert.IsTrue(handler.CapturedRequests.Any(r => r.Path == "/api/v1/jobs/job-88/outputs/0"));
            Assert.IsTrue(handler.CapturedRequests.Any(r => r.Path == "/api/v1/jobs/job-88/outputs/1"));
        }

        [TestMethod]
        public async Task GetImages_FailedJob_ThrowsDomainException()
        {
            var handler = new QueueMessageHandler();
            handler.EnqueueJson(HttpStatusCode.OK,
                "{ \"status\": \"Failed\", \"errorMessage\": \"Bad prompt\" }");

            var client = CreateClient(handler);

            var ex = await Assert.ThrowsExceptionAsync<ComfyBridgeJobFailedException>(
                () => client.GetImages(new ResultGenerate { Id = "job-fail" }));

            StringAssert.Contains(ex.Message, "Bad prompt");
        }

        [TestMethod]
        public async Task Remove_IsNoOp_ForValidId()
        {
            var handler = new QueueMessageHandler();
            var client = CreateClient(handler);

            await client.Remove(new ResultGenerate { Id = "job-keep" });

            Assert.AreEqual(0, handler.CapturedRequests.Count);
        }

        [TestMethod]
        public async Task GetModels_MapsTemplatesToImageModels()
        {
            var handler = new QueueMessageHandler();
            handler.EnqueueJson(HttpStatusCode.OK,
                """
                {
                  "templates": [
                    {
                      "name": "text2image",
                      "version": "1.2",
                      "category": "image",
                      "inputs": {
                        "prompt": { "type": "string" },
                        "model": { "type": "string" }
                      }
                    },
                    {
                      "name": "upscaler",
                      "version": "2.0",
                      "category": "postprocess",
                      "inputs": {
                        "image": { "type": "string" }
                      }
                    }
                  ]
                }
                """);

            var client = CreateClient(handler);
            var models = await client.GetModels();

            Assert.AreEqual(2, models.Count);
            Assert.AreEqual("image:text2image:1.2", models[0].Id);
            Assert.AreEqual("text2image (1.2)", models[0].Name);
            Assert.IsTrue(models[0].Description?.Contains("supports model field", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual("postprocess:upscaler:2.0", models[1].Id);
            Assert.AreEqual("upscaler (2.0)", models[1].Name);
        }

        private static ComfyBridgeImageGeneratorClient CreateClient(
            QueueMessageHandler handler,
            Action<ComfyBridgeClientOptions>? configureOptions = null)
        {
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var optionsModel = new ComfyBridgeClientOptions
            {
                BaseUrl = "http://localhost",
                PollIntervalMs = 1,
                JobTimeoutSeconds = 5
            };

            configureOptions?.Invoke(optionsModel);

            var options = Options.Create(optionsModel);

            return new ComfyBridgeImageGeneratorClient(
                httpClient,
                options,
                NullLogger<ComfyBridgeImageGeneratorClient>.Instance);
        }

        private sealed class QueueMessageHandler : HttpMessageHandler
        {
            private readonly Queue<Func<HttpResponseMessage>> _responses = [];

            public List<CapturedRequest> CapturedRequests { get; } = [];

            public void EnqueueJson(HttpStatusCode statusCode, string json)
            {
                _responses.Enqueue(() => new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }

            public void EnqueueBinary(HttpStatusCode statusCode, byte[] bytes)
            {
                _responses.Enqueue(() => new HttpResponseMessage(statusCode)
                {
                    Content = new ByteArrayContent(bytes)
                });
            }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                if (_responses.Count == 0)
                {
                    throw new InvalidOperationException("No mocked responses configured.");
                }

                var body = request.Content == null
                    ? null
                    : await request.Content.ReadAsStringAsync(cancellationToken);

                CapturedRequests.Add(new CapturedRequest(
                    request.Method,
                    request.RequestUri?.AbsolutePath ?? string.Empty,
                    body));

                return _responses.Dequeue().Invoke();
            }
        }

        private sealed record CapturedRequest(HttpMethod Method, string Path, string? Body);
    }
}