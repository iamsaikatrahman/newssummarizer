using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;

namespace NewsSummarizer.Services;

public class NewsScraperService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsScraperService> _logger;

    public NewsScraperService(HttpClient httpClient, ILogger<NewsScraperService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<(string title, string content)> ScrapeArticleAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove script, style, nav, footer, header, aside nodes
            var nodesToRemove = doc.DocumentNode
                .SelectNodes("//script|//style|//nav|//footer|//header|//aside|//form|//iframe|//noscript|//ads")
                ?? new HtmlNodeCollection(null);

            foreach (var node in nodesToRemove.ToList())
                node.Remove();

            // Extract title
            var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim()
                     ?? doc.DocumentNode.SelectSingleNode("//h1")?.InnerText?.Trim()
                     ?? "Unknown Title";

            // Clean title
            title = HtmlEntity.DeEntitize(title);

            // Try to extract article body using common selectors
            var articleSelectors = new[]
            {
                "//article",
                "//*[@class='article-body']",
                "//*[@class='post-content']",
                "//*[@class='entry-content']",
                "//*[@class='story-body']",
                "//*[@itemprop='articleBody']",
                "//*[@class='content']",
                "//main"
            };

            HtmlNode? articleNode = null;
            foreach (var selector in articleSelectors)
            {
                articleNode = doc.DocumentNode.SelectSingleNode(selector);
                if (articleNode != null) break;
            }

            string content;
            if (articleNode != null)
            {
                content = ExtractText(articleNode);
            }
            else
            {
                // Fallback: get all paragraph text
                var paragraphs = doc.DocumentNode.SelectNodes("//p");
                if (paragraphs != null)
                {
                    var sb = new StringBuilder();
                    foreach (var p in paragraphs)
                    {
                        var text = HtmlEntity.DeEntitize(p.InnerText).Trim();
                        if (text.Length > 50) // Filter short/nav paragraphs
                            sb.AppendLine(text);
                    }
                    content = sb.ToString();
                }
                else
                {
                    content = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
                }
            }

            // Clean up whitespace
            content = Regex.Replace(content, @"\s{3,}", "\n\n").Trim();

            // Limit content to ~8000 chars to stay within token limits
            if (content.Length > 8000)
                content = content[..8000] + "...";

            return (title, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping URL: {Url}", url);
            throw new Exception($"Failed to fetch article: {ex.Message}");
        }
    }

    private static string ExtractText(HtmlNode node)
    {
        var sb = new StringBuilder();
        foreach (var child in node.DescendantsAndSelf())
        {
            if (child.NodeType == HtmlNodeType.Text)
            {
                var text = HtmlEntity.DeEntitize(child.InnerText).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    sb.AppendLine(text);
            }
        }
        return sb.ToString();
    }
}