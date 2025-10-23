using Microsoft.Extensions.Options;
using NetXP;
using NetXP.Translators.AzureTranslator;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class AzureTranslatorImplementation : ITranslator
{
    public AzureTranslatorImplementation(IOptions<AzureTranslatorOptions> options)
    {
        Options = options;
        //validate the token
        if (string.IsNullOrEmpty(Options.Value.Token))
        {
            throw new ArgumentException("Azure Translator Token is not set.");
        }
    }

    public IOptions<AzureTranslatorOptions> Options { get; }

    public async Task<string> TranslateTextAsync(string text, string toLanguage)
    {
        string route = $"/translate?api-version=3.0&to={toLanguage}";
        object[] body = [new { Text = text }];
        var requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(body);

        using var client = new HttpClient();
        using var request = new HttpRequestMessage();

        request.Method = HttpMethod.Post;
        request.RequestUri = new Uri(Options.Value.EndPoint + route);
        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        request.Headers.Add("Ocp-Apim-Subscription-Key", Options.Value.Token);
        request.Headers.Add("Ocp-Apim-Subscription-Region", Options.Value.Region);

        HttpResponseMessage response = await client.SendAsync(request);
        string result = await response.Content.ReadAsStringAsync();
        var json = JArray.Parse(result);
        //check if the message is like this: {"error":{"code":401001,"message":"The request is not authorized because credentials are missing or invalid."}}

        return json[0]["translations"][0]["text"].ToString();
    }
}

