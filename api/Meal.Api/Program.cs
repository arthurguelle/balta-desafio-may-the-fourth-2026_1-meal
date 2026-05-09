using Meal.Ai;
using Meal.Ai.Contracts;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCors";

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
	?? ["http://localhost:5282", "https://localhost:7277"];

builder.Services.AddLocalAi(builder.Configuration);
builder.Services.AddCors(options =>
{
	options.AddPolicy(FrontendCorsPolicy, policy =>
	{
		policy
			.WithOrigins(allowedOrigins)
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

var app = builder.Build();

app.UseCors(FrontendCorsPolicy);

app.MapGet("/", () => Results.Ok(new
{
	service = "Meal.Api",
	status = "running"
}));

app.MapGet("/health/ai", async (IRecipeSuggestionClient aiClient, CancellationToken cancellationToken) =>
{
	var healthy = await aiClient.IsHealthyAsync(cancellationToken);

	return healthy
		? Results.Ok(new { service = "ai", status = "healthy" })
		: Results.Problem(
			title: "IA local indisponivel",
			detail: "Nao foi possivel validar a IA local configurada.",
			statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapPost("/recipes/suggest", async (SuggestRecipeRequest request, IRecipeSuggestionClient aiClient, CancellationToken cancellationToken) =>
{
	if (request.AvailableMinutes <= 0)
		return Results.BadRequest(new { error = "AvailableMinutes deve ser maior que zero." });

	if (string.IsNullOrWhiteSpace(request.Ingredients))
		return Results.BadRequest(new { error = "Informe os ingredientes disponiveis." });

	try
	{
		var suggestion = await aiClient.SuggestRecipeAsync(
			ingredients: request.Ingredients,
			availableMinutes: request.AvailableMinutes,
			cancellationToken: cancellationToken);
		return Results.Ok(new { recipe = suggestion });
	}
	catch (Exception ex)
	{
		return Results.Problem(
			title: "Falha ao gerar receita",
			detail: ex.Message,
			statusCode: StatusCodes.Status503ServiceUnavailable);
	}
});

app.Run();

public sealed class SuggestRecipeRequest
{
	public string? Ingredients { get; init; }
	public int AvailableMinutes { get; init; }
}
