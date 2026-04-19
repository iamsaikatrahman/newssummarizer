namespace NewsSummarizer.Models;

public class SummaryResult
{
    public bool IsSuccess { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? KeyPoints { get; set; }
    public string? Sentiment { get; set; }
    public string? Category { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}