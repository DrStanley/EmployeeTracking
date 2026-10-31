using EmployeeTracking.Application.ScreenCapture;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeTracking.Infrastructure.ScreenCapture
{
    public static class ScreenCaptureServiceCollectionExtensions
    {
        public static IServiceCollection AddScreenCapture(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ScreenCaptureOptions>(config.GetSection("ScreenCapture"));
            var storage = config["ScreenCapture:Storage"];
            var ai = config["ScreenCapture:Ai"];

            if (string.Equals(storage, "Azure", StringComparison.OrdinalIgnoreCase))
                services.AddSingleton<IScreenImageStorage, AzureBlobScreenImageStorage>();
            else if (string.Equals(storage, "Aws", StringComparison.OrdinalIgnoreCase))
                services.AddSingleton<IScreenImageStorage, AwsS3ScreenImageStorage>();
            else
                services.AddSingleton<IScreenImageStorage, SimulatedScreenImageStorage>();

            if (string.Equals(ai, "Grok", StringComparison.OrdinalIgnoreCase))
                services.AddHttpClient<IScreenAiAnalyzer, GrokScreenAiAnalyzer>();
            else if (string.Equals(ai, "Gemini", StringComparison.OrdinalIgnoreCase))
                services.AddHttpClient<IScreenAiAnalyzer, GeminiScreenAiAnalyzer>();
            else if (string.Equals(ai, "OpenAi", StringComparison.OrdinalIgnoreCase))
                services.AddHttpClient<IScreenAiAnalyzer, OpenAiScreenAiAnalyzer>();
            else
                services.AddSingleton<IScreenAiAnalyzer, SimulatedScreenAiAnalyzer>();

            return services;
        }
    }
}
