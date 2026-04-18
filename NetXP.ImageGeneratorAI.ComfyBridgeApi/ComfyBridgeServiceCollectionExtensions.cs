using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NetXP.IAs.ImageGeneratorAI;

namespace NetXP.ImageGeneratorAI.ComfyBridgeApi
{
    public static class ComfyBridgeServiceCollectionExtensions
    {
        public static IServiceCollection AddComfyBridgeImageGenerator(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<ComfyBridgeClientOptions>()
                .Configure(options => configuration.GetSection(ComfyBridgeClientOptions.SectionName).Bind(options));

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
    }
}