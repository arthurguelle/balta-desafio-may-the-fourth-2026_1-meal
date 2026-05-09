using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Meal.Frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5230/";
builder.Services.AddHttpClient("MealApi", client => client.BaseAddress = new Uri(apiBaseUrl));

await builder.Build().RunAsync();
