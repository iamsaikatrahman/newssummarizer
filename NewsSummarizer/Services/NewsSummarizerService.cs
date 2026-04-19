#region OpenAPI
//using OpenAI;
//using OpenAI.Chat;
//using NewsSummarizer.Models;
//using System.Text.Json;

//namespace NewsSummarizer.Services;

//public class NewsSummarizerService
//{
//    private readonly IConfiguration _configuration;
//    private readonly ILogger<NewsSummarizerService> _logger;
//    private readonly NewsScraperService _scraperService;

//    public NewsSummarizerService(
//        IConfiguration configuration,
//        ILogger<NewsSummarizerService> logger,
//        NewsScraperService scraperService)
//    {
//        _configuration = configuration;
//        _logger = logger;
//        _scraperService = scraperService;
//    }

//    public async Task<SummaryResult> SummarizeNewsAsync(string url)
//    {
//        var result = new SummaryResult { SourceUrl = url };

//        try
//        {
//            // Validate URL
//            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
//                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
//            {
//                return new SummaryResult
//                {
//                    IsSuccess = false,
//                    ErrorMessage = "Please enter a valid HTTP/HTTPS URL."
//                };
//            }

//            // Scrape article content
//            var (title, content) = await _scraperService.ScrapeArticleAsync(url);

//            if (string.IsNullOrWhiteSpace(content) || content.Length < 100)
//            {
//                return new SummaryResult
//                {
//                    IsSuccess = false,
//                    ErrorMessage = "Could not extract meaningful content from the provided URL. The page may require JavaScript or have restricted access."
//                };
//            }

//            // Call OpenAI
//            var apiKey = _configuration["OpenAI:ApiKey"]
//                ?? throw new InvalidOperationException("OpenAI API key not configured.");
//            var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";

//            var openAiClient = new OpenAIClient(apiKey);
//            var chatClient = openAiClient.GetChatClient(model);

//            var systemPrompt = """
//                You are a professional news analyst. When given a news article, you provide:
//                1. A concise, engaging summary (3-4 sentences)
//                2. 4-5 key bullet points
//                3. Sentiment analysis (Positive / Neutral / Negative)
//                4. Article category (Politics, Technology, Business, Health, Sports, Entertainment, Science, World, etc.)

//                Respond ONLY with valid JSON in this exact format:
//                {
//                  "summary": "...",
//                  "keyPoints": ["point1", "point2", "point3", "point4"],
//                  "sentiment": "Positive|Neutral|Negative",
//                  "category": "..."
//                }
//                """;

//            var userPrompt = $"Article Title: {title}\n\nArticle Content:\n{content}";

//            var messages = new List<ChatMessage>
//            {
//                new SystemChatMessage(systemPrompt),
//                new UserChatMessage(userPrompt)
//            };

//            var chatResult = await chatClient.CompleteChatAsync(messages);
//            var responseText = chatResult.Value.Content[0].Text;

//            // Parse JSON response
//            var jsonStart = responseText.IndexOf('{');
//            var jsonEnd = responseText.LastIndexOf('}');
//            if (jsonStart >= 0 && jsonEnd >= 0)
//                responseText = responseText[jsonStart..(jsonEnd + 1)];

//            var parsed = JsonSerializer.Deserialize<OpenAiResponse>(responseText,
//                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

//            result.IsSuccess = true;
//            result.Title = title;
//            result.Summary = parsed?.Summary ?? "No summary available.";
//            result.KeyPoints = parsed?.KeyPoints != null
//                ? string.Join("|", parsed.KeyPoints)
//                : string.Empty;
//            result.Sentiment = parsed?.Sentiment ?? "Neutral";
//            result.Category = parsed?.Category ?? "General";
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error summarizing news for URL: {Url}", url);
//            result.IsSuccess = false;
//            result.ErrorMessage = ex.Message.Contains("API key")
//                ? "Invalid or missing OpenAI API key. Please check your configuration."
//                : $"An error occurred: {ex.Message}";
//        }

//        return result;
//    }

//    private class OpenAiResponse
//    {
//        public string? Summary { get; set; }
//        public List<string>? KeyPoints { get; set; }
//        public string? Sentiment { get; set; }
//        public string? Category { get; set; }
//    }
//}

#endregion

#region GemniAPI
//using NewsSummarizer.Models;
//using System.Text;
//using System.Text.Json;

//namespace NewsSummarizer.Services;

//public class NewsSummarizerService
//{
//    private readonly IConfiguration _configuration;
//    private readonly ILogger<NewsSummarizerService> _logger;
//    private readonly NewsScraperService _scraperService;
//    private readonly HttpClient _httpClient;

//    public NewsSummarizerService(
//        IConfiguration configuration,
//        ILogger<NewsSummarizerService> logger,
//        NewsScraperService scraperService,
//        IHttpClientFactory httpClientFactory)
//    {
//        _configuration = configuration;
//        _logger = logger;
//        _scraperService = scraperService;
//        _httpClient = httpClientFactory.CreateClient();
//    }

//    public async Task<SummaryResult> SummarizeNewsAsync(string url)
//    {
//        var result = new SummaryResult { SourceUrl = url };

//        try
//        {
//            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
//                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
//            {
//                return new SummaryResult
//                {
//                    IsSuccess = false,
//                    ErrorMessage = "Please enter a valid HTTP/HTTPS URL."
//                };
//            }

//            var (title, content) = await _scraperService.ScrapeArticleAsync(url);

//            if (string.IsNullOrWhiteSpace(content) || content.Length < 100)
//            {
//                return new SummaryResult
//                {
//                    IsSuccess = false,
//                    ErrorMessage = "Could not extract meaningful content from the provided URL. The page may require JavaScript or have restricted access."
//                };
//            }

//            var apiKey = _configuration["Gemini:ApiKey"]
//                ?? throw new InvalidOperationException("Gemini API key not configured in appsettings.json.");
//            var model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";

//            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

//            var prompt = $@"You are a professional news analyst. Analyze the following news article and respond ONLY with valid JSON — no markdown, no code fences, no extra text.

//JSON format required:
//{{
//  ""summary"": ""A concise 3-4 sentence summary of the article"",
//  ""keyPoints"": [""point 1"", ""point 2"", ""point 3"", ""point 4""],
//  ""sentiment"": ""Positive"",
//  ""category"": ""Technology""
//}}

//Rules:
//- sentiment must be exactly one of: Positive, Neutral, Negative
//- category must be one of: Politics, Technology, Business, Health, Sports, Entertainment, Science, World, Finance, Environment
//- keyPoints must have 4-5 items
//- Output ONLY the JSON object, nothing else

//Article Title: {title}

//Article Content:
//{content}";

//            var requestBody = new
//            {
//                contents = new[]
//                {
//                    new { parts = new[] { new { text = prompt } } }
//                },
//                generationConfig = new
//                {
//                    temperature = 0.3,
//                    maxOutputTokens = 1024
//                }
//            };

//            var json = JsonSerializer.Serialize(requestBody);
//            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
//            var response = await _httpClient.PostAsync(endpoint, httpContent);
//            var responseBody = await response.Content.ReadAsStringAsync();

//            if (!response.IsSuccessStatusCode)
//            {
//                try
//                {
//                    var errorDoc = JsonDocument.Parse(responseBody);
//                    var errorMsg = errorDoc.RootElement.GetProperty("error").GetProperty("message").GetString();
//                    throw new Exception($"Gemini API error: {errorMsg}");
//                }
//                catch (Exception ex) when (!ex.Message.StartsWith("Gemini"))
//                {
//                    throw new Exception($"Gemini API returned {response.StatusCode}: {responseBody}");
//                }
//            }

//            var responseDoc = JsonDocument.Parse(responseBody);
//            var responseText = responseDoc.RootElement
//                .GetProperty("candidates")[0]
//                .GetProperty("content")
//                .GetProperty("parts")[0]
//                .GetProperty("text")
//                .GetString() ?? string.Empty;

//            responseText = responseText.Trim();
//            if (responseText.StartsWith("```"))
//            {
//                var firstNewline = responseText.IndexOf('\n');
//                var lastFence = responseText.LastIndexOf("```");
//                if (firstNewline >= 0 && lastFence > firstNewline)
//                    responseText = responseText[(firstNewline + 1)..lastFence].Trim();
//            }

//            var jsonStart = responseText.IndexOf('{');
//            var jsonEnd = responseText.LastIndexOf('}');
//            if (jsonStart >= 0 && jsonEnd >= 0)
//                responseText = responseText[jsonStart..(jsonEnd + 1)];

//            var parsed = JsonSerializer.Deserialize<GeminiResponse>(responseText,
//                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

//            result.IsSuccess = true;
//            result.Title = title;
//            result.Summary = parsed?.Summary ?? "No summary available.";
//            result.KeyPoints = parsed?.KeyPoints != null ? string.Join("|", parsed.KeyPoints) : string.Empty;
//            result.Sentiment = parsed?.Sentiment ?? "Neutral";
//            result.Category = parsed?.Category ?? "General";
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error summarizing news for URL: {Url}", url);
//            result.IsSuccess = false;
//            var msg = ex.Message;
//            result.ErrorMessage = msg.Contains("API key not configured")
//                ? "Gemini API key is missing. Please add it to appsettings.json under Gemini:ApiKey."
//                : msg.Contains("API_KEY_INVALID") || msg.Contains("API key not valid")
//                    ? "Invalid Gemini API key. Please check your key at aistudio.google.com."
//                    : $"An error occurred: {msg}";
//        }

//        return result;
//    }

//    private class GeminiResponse
//    {
//        public string? Summary { get; set; }
//        public List<string>? KeyPoints { get; set; }
//        public string? Sentiment { get; set; }
//        public string? Category { get; set; }
//    }
//}
#endregion

#region GroqAPI
using NewsSummarizer.Models;
using System.Text;
using System.Text.Json;

namespace NewsSummarizer.Services;

public class NewsSummarizerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsSummarizerService> _logger;
    private readonly NewsScraperService _scraperService;
    private readonly HttpClient _httpClient;

    public NewsSummarizerService(
        IConfiguration configuration,
        ILogger<NewsSummarizerService> logger,
        NewsScraperService scraperService,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _scraperService = scraperService;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<SummaryResult> SummarizeNewsAsync(string url)
    {
        var result = new SummaryResult { SourceUrl = url };

        try
        {
            // Validate URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return new SummaryResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Please enter a valid HTTP/HTTPS URL."
                };
            }

            // Scrape article content
            var (title, content) = await _scraperService.ScrapeArticleAsync(url);

            if (string.IsNullOrWhiteSpace(content) || content.Length < 100)
            {
                return new SummaryResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Could not extract meaningful content from the provided URL. The page may require JavaScript or have restricted access."
                };
            }

            // Get Groq config
            var apiKey = _configuration["Groq:ApiKey"]
                ?? throw new InvalidOperationException("Groq API key not configured in appsettings.json.");
            var model = _configuration["Groq:Model"] ?? "llama-3.3-70b-versatile";

            // Groq uses OpenAI-compatible API
            var endpoint = "https://api.groq.com/openai/v1/chat/completions";

            var systemPrompt = "You are a professional news analyst. Respond ONLY with valid JSON — no markdown, no code fences, no explanation, no extra text. Just the raw JSON object.";

            // Trim article content before sending to API
            var trimmedContent = content.Length > 4000 ? content[..4000] : content;

            var userPrompt = $@"Analyze the following news article and return ONLY this JSON structure:
{{
  ""summary"": ""10-20 sentence summary here"",
  ""keyPoints"": [""point 1"", ""point 2"", ""point 3"", ""point 4""],
  ""sentiment"": ""Positive"",
  ""category"": ""Technology""
}}

Rules:
- summary must be 10-20 sentences long
- summary MUST be written in the SAME language as the article title. If the title is in Bengali, write the summary in Bengali. If the title is in English, write in English. Match the language exactly.
- keyPoints MUST also be written in the SAME language as the article title
- sentiment must be exactly: Positive, Neutral, or Negative
- category must be one of: Politics, Technology, Business, Health, Sports, Entertainment, Science, World, Finance, Environment
- Return ONLY the JSON object, nothing else, no markdown, no code fences

Article Title: {title}

Article Content:
{trimmedContent}";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userPrompt   }
                },
                temperature = 0.3,
                max_tokens = 2048
            };

            var json = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync(endpoint, httpContent);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorDoc = JsonDocument.Parse(responseBody);
                    var errorMsg = errorDoc.RootElement
                        .GetProperty("error")
                        .GetProperty("message")
                        .GetString();
                    throw new Exception($"Groq API error: {errorMsg}");
                }
                catch (Exception ex) when (!ex.Message.StartsWith("Groq"))
                {
                    throw new Exception($"Groq API returned {response.StatusCode}: {responseBody}");
                }
            }

            // Parse OpenAI-compatible response
            var responseDoc = JsonDocument.Parse(responseBody);
            var responseText = responseDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            // Strip markdown fences if model adds them anyway
            responseText = responseText.Trim();
            if (responseText.StartsWith("```"))
            {
                var firstNewline = responseText.IndexOf('\n');
                var lastFence = responseText.LastIndexOf("```");
                if (firstNewline >= 0 && lastFence > firstNewline)
                    responseText = responseText[(firstNewline + 1)..lastFence].Trim();
            }

            // Extract JSON object
            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd >= 0)
                responseText = responseText[jsonStart..(jsonEnd + 1)];

            var parsed = JsonSerializer.Deserialize<GroqResponse>(responseText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.IsSuccess = true;
            result.Title = title;
            result.Summary = parsed?.Summary ?? "No summary available.";
            result.KeyPoints = parsed?.KeyPoints != null
                ? string.Join("|", parsed.KeyPoints)
                : string.Empty;
            result.Sentiment = parsed?.Sentiment ?? "Neutral";
            result.Category = parsed?.Category ?? "General";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error summarizing news for URL: {Url}", url);
            result.IsSuccess = false;
            var msg = ex.Message;
            result.ErrorMessage = msg.Contains("API key not configured")
                ? "Groq API key is missing. Please add it to appsettings.json under Groq:ApiKey."
                : msg.Contains("Invalid API Key") || msg.Contains("401")
                    ? "Invalid Groq API key. Please check your key at console.groq.com."
                    : $"An error occurred: {msg}";
        }

        return result;
    }

    private class GroqResponse
    {
        public string? Summary { get; set; }
        public List<string>? KeyPoints { get; set; }
        public string? Sentiment { get; set; }
        public string? Category { get; set; }
    }
}
#endregion
