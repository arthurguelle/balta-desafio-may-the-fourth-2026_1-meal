using Meal.Ai.Contracts;
using Meal.Ai.LmStudio;
using Meal.Ai.Ollama;
using Meal.Ai.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Meal.Ai;

public static class DependencyInjection
{
    public static IServiceCollection AddLocalAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection("AI"));

        var aiOptions = configuration.GetSection("AI").Get<AiOptions>() ?? new AiOptions();
        var provider = aiOptions.Provider.Trim().ToLowerInvariant();

        if (provider == "ollama")
        {
            services.AddHttpClient<IRecipeSuggestionClient, OllamaRecipeSuggestionClient>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AiOptions>>().Value;
                var baseUrl = options.BaseUrl.TrimEnd('/') + "/";

                httpClient.BaseAddress = new Uri(baseUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds));
            });

            return services;
        }

        services.AddHttpClient<IRecipeSuggestionClient, LmStudioRecipeSuggestionClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AiOptions>>().Value;
            var baseUrl = options.BaseUrl.TrimEnd('/') + "/";

            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds));
        });

        return services;
    }
}
