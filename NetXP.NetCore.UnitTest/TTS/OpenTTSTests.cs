using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NetXP.CompositionRoots;
using NetXP.DependencyInjection;
using NetXP.DependencyInjection.Implementations.StructureMaps;
using NetXP.ImageGeneratorAI;
using NetXP.ImageGeneratorAI.LeonardoAI;
using NetXP.Processes;
using NetXP.Processes.Implementations;
using StructureMap;
using System;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading;
using Moq.Contrib.HttpClient;
using Newtonsoft.Json;
using NetXP.TTSs.OpenTTS;
using NetXP.TTS;
using System.Collections.Generic;
using System.IO;

namespace NetXP.NetCoreUnitTest.Processes.Implementations
{
    [TestClass]
    public class OpenTTSTests
    {
        public DependencyInjection.IContainer container;

        [TestMethod]
        public void GetTTSVoices()
        {
            //Init options
            var opt = new Mock<IOptions<TTSOptions>>();
            opt.Setup(o => o.Value).Returns(new TTSOptions { URL = "http://localhost/" });

            //Init http
            var handler = new Mock<HttpMessageHandler>();
            var client = handler.CreateClient();
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(o => o.CreateClient(string.Empty)).Returns(client);

            //This string was generated by Leonardo.AI API
            string jsonPostResult = @"{
                                        ""espeak:es"": {
                                            ""gender"": ""M"",
                                            ""id"": ""es"",
                                            ""language"": ""es"",
                                            ""locale"": ""es"",
                                            ""multispeaker"": false,
                                            ""name"": ""Spanish_(Spain)"",
                                            ""speakers"": null,
                                            ""tag"": null,
                                            ""tts_name"": ""espeak""
                                        },
                                        ""espeak:es-419"": {
                                            ""gender"": ""M"",
                                            ""id"": ""es-419"",
                                            ""language"": ""es"",
                                            ""locale"": ""es-419"",
                                            ""multispeaker"": false,
                                            ""name"": ""Spanish_(Latin_America)"",
                                            ""speakers"": null,
                                            ""tag"": null,
                                            ""tts_name"": ""espeak""
                                        }}";

            var dynamicPostResult = JsonConvert.DeserializeObject<Dictionary<string, OpenTtsModelResponse>>(jsonPostResult);
            handler.SetupAnyRequest()
                   .ReturnsResponse(jsonPostResult);

            var leoAI = new TTSOpenTTS(opt.Object, clientFactory.Object);

            //Do
            var result = leoAI.GetTTSVoices("es").Result;

            //Assert
            Assert.AreEqual(result.Count, dynamicPostResult.Count);
        }

        [TestMethod]
        public void Convert()
        {
            //Init options
            var opt = new Mock<IOptions<TTSOptions>>();
            opt.Setup(o => o.Value).Returns(new TTSOptions { URL = "http://localhost:5500/" });

            //Init http
            var handler = new Mock<HttpMessageHandler>();
            var client = handler.CreateClient();
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(o => o.CreateClient()).Returns(client);


            var expectedFile = File.ReadAllBytes("audio.wav");

            handler.SetupAnyRequest()
                  .ReturnsResponse(new MemoryStream(expectedFile, 0, expectedFile.Length));

            //Init options
            var leoAI = new TTSOpenTTS(opt.Object, clientFactory.Object);

            //Do
            var result = leoAI.Convert(new TTSConvertOption
            {
                Text = "Hola, probando.",
                Voice = new TTSVoice
                {
                    Gender = "F",
                    Id = "es-ES",
                    Language = "es",
                    Name = "es-ES",
                    ModelId = "nanotts"
                }
            }).Result;

            var responseFile = result.File.ToArray();

            //Assert
            Assert.AreEqual(responseFile.Length, expectedFile.Length);
            Assert.IsTrue(responseFile.SequenceEqual(expectedFile));
        }
    }
}