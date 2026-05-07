using System.Net.Http.Json;
using System.Text.Json;
using Meal.Ai.Contracts;
using Meal.Ai.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meal.Ai.LmStudio;

public sealed class LmStudioRecipeSuggestionClient : IRecipeSuggestionClient
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly ILogger<LmStudioRecipeSuggestionClient> _logger;

    public LmStudioRecipeSuggestionClient(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        ILogger<LmStudioRecipeSuggestionClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("v1/models", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check da IA local falhou em {BaseUrl}", _options.BaseUrl);
            return false;
        }
    }

    public async Task<string> SuggestRecipeAsync(
        string ingredients,
        int availableMinutes,
        string? pageContext = null,
        string? finalPrompt = null,
        CancellationToken cancellationToken = default)
    {
        if (availableMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(availableMinutes), "Tempo disponível deve ser maior que zero.");

        var prompt = BuildPrompt(ingredients, availableMinutes, pageContext, finalPrompt);

        var requestPayload = new
        {
            model = _options.Model,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "Voce e um assistente culinario. Sugira uma receita objetiva em portugues, com ingredientes e passos numerados."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = 0.4,
            max_tokens = 500
        };

        var maxAttempts = Math.Max(1, _options.RetryCount + 1);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", requestPayload, cancellationToken);
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var shouldRetry = (int)response.StatusCode >= 500 && attempt < maxAttempts;
                    if (shouldRetry)
                    {
                        _logger.LogWarning("Tentativa {Attempt}/{MaxAttempts} falhou no LM Studio com status {StatusCode}.", attempt, maxAttempts, response.StatusCode);
                        continue;
                    }

                    throw new InvalidOperationException($"Falha ao consultar IA local ({(int)response.StatusCode}): {raw}");
                }

                using var document = JsonDocument.Parse(raw);
                var content = document
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(content))
                    throw new InvalidOperationException("A IA local retornou resposta vazia.");

                return content.Trim();
            }
            catch (Exception ex) when (attempt < maxAttempts && (ex is HttpRequestException || ex is TaskCanceledException))
            {
                _logger.LogWarning(ex, "Tentativa {Attempt}/{MaxAttempts} de consulta ao LM Studio falhou.", attempt, maxAttempts);
            }
        }

        throw new InvalidOperationException("Nao foi possivel obter uma sugestao da IA local apos as tentativas configuradas.");
    }

    private static string BuildPrompt(string ingredients, int availableMinutes, string? pageContext, string? finalPrompt)
    {
        if (!string.IsNullOrWhiteSpace(finalPrompt))
            return finalPrompt.Trim();

        if (string.IsNullOrWhiteSpace(ingredients))
            throw new ArgumentException("Ingredientes devem ser informados quando nenhum prompt final e enviado.", nameof(ingredients));

        var prompt = $"Tenho os ingredientes: {ingredients}. Tenho {availableMinutes} minutos livres. Sugira a melhor receita para esse tempo.";

        if (!string.IsNullOrWhiteSpace(pageContext))
        {
            var normalizedContext = pageContext.Trim();
            if (normalizedContext.Length > 2000)
                normalizedContext = normalizedContext[..2000];

            prompt += $" Contexto adicional da pagina: {normalizedContext}.";
        }

        return prompt;
    }
}
