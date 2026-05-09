namespace Meal.Ai.Contracts;

public interface IRecipeSuggestionClient
{
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    Task<string> SuggestRecipeAsync(
        string ingredients,
        int availableMinutes,
        CancellationToken cancellationToken = default);
}
