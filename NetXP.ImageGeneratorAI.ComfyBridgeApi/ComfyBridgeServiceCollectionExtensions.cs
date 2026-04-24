using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NetXP.IAs.ImageGeneratorAI;
using System.Globalization;

namespace NetXP.ImageGeneratorAI.ComfyBridgeApi
{
    public static class ComfyBridgeServiceCollectionExtensions
    {
        public static IServiceCollection AddComfyBridgeImageGenerator(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<ComfyBridgeClientOptions>()
                .Configure(options => {
                    configuration.GetSection(ComfyBridgeClientOptions.SectionName).Bind(options);

                    var sharedExtraOptions = configuration
                        .GetSection("ImageGeneratorAIOptions:ExtraOptions")
                        .GetChildren();

                    foreach (var extraOption in sharedExtraOptions)
                    {
                        if (string.IsNullOrWhiteSpace(extraOption.Key)
                            || options.ExtraOptions.ContainsKey(extraOption.Key))
                        {
                            continue;
                        }

                        options.ExtraOptions[extraOption.Key] = ParseConfigurationValue(extraOption.Value);
                    }
                   });

            services.AddHttpClient<IImageGeneratorAI, ComfyBridgeImageGeneratorClient>((serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<ComfyBridgeClientOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            });

            services.TryAddTransient<ComfyBridgeImageGeneratorClient>();

            return services;
        }

        private static object? ParseConfigurationValue(string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return rawValue;
            }

            if (bool.TryParse(rawValue, out var boolValue))
            {
                return boolValue;
            }

            if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
            {
                return longValue;
            }

            if (decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
            {
                return decimalValue;
            }

            return rawValue;
        }
    }
}