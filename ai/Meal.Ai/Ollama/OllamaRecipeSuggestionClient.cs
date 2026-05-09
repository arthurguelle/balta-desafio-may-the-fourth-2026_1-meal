using System.Net.Http.Json;
using System.Text.Json;
using Meal.Ai.Contracts;
using Meal.Ai.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meal.Ai.Ollama;

public sealed class OllamaRecipeSuggestionClient : IRecipeSuggestionClient
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly ILogger<OllamaRecipeSuggestionClient> _logger;

    public OllamaRecipeSuggestionClient(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        ILogger<OllamaRecipeSuggestionClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("api/tags", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check do Ollama falhou em {BaseUrl}", _options.BaseUrl);
            return false;
        }
    }

    public async Task<string> SuggestRecipeAsync(
        string ingredients,
        int availableMinutes,
        CancellationToken cancellationToken = default)
    {
        if (availableMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(availableMinutes), "Tempo disponivel deve ser maior que zero.");

        var prompt = BuildPrompt(ingredients, availableMinutes);

        var payload = new
        {
            model = _options.Model,
            prompt,
            stream = false,
            options = new
            {
                temperature = 0.4
            }
        };

        var maxAttempts = Math.Max(1, _options.RetryCount + 1);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var response = await _httpClient.PostAsJsonAsync("api/generate", payload, cancellationToken);
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var shouldRetry = (int)response.StatusCode >= 500 && attempt < maxAttempts;
                    if (shouldRetry)
                    {
                        _logger.LogWarning("Tentativa {Attempt}/{MaxAttempts} falhou no Ollama com status {StatusCode}.", attempt, maxAttempts, response.StatusCode);
                        continue;
                    }

                    throw new InvalidOperationException($"Falha ao consultar Ollama ({(int)response.StatusCode}): {raw}");
                }

                using var document = JsonDocument.Parse(raw);
                var content = document.RootElement.GetProperty("response").GetString();

                if (string.IsNullOrWhiteSpace(content))
                    throw new InvalidOperationException("O Ollama retornou resposta vazia.");

                return content.Trim();
            }
            catch (Exception ex) when (attempt < maxAttempts && (ex is HttpRequestException || ex is TaskCanceledException))
            {
                _logger.LogWarning(ex, "Tentativa {Attempt}/{MaxAttempts} de consulta ao Ollama falhou.", attempt, maxAttempts);
            }
        }

        throw new InvalidOperationException("Nao foi possivel obter uma sugestao da IA local apos as tentativas configuradas.");
    }

    private static string BuildPrompt(string ingredients, int availableMinutes)
    {
        if (string.IsNullOrWhiteSpace(ingredients))
            throw new ArgumentException("Ingredientes devem ser informados.", nameof(ingredients));

        return
            "Voce e um assistente culinario. " +
            "Responda em portugues do Brasil com uma receita objetiva, use apenas os ingredientes listados, inclua ingredientes e passos numerados. " +
            $"Ingredientes disponiveis: {ingredients.Trim()}. " +
            $"Tempo maximo de preparo: {availableMinutes} minutos.";
    }
}
