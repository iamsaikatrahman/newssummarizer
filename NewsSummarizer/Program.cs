using NewsSummarizer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register HttpClient factory (used by both services)
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<NewsScraperService>();

// Register services
builder.Services.AddScoped<NewsScraperService>();
builder.Services.AddScoped<NewsSummarizerService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<NewsSummarizer.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();