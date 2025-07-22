namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents test plan totals/statistics
/// </summary>
public record TestPlanTotal
{
    public string Type { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int TotalTestCases { get; init; }
    public Dictionary<string, int> Details { get; init; } = new();
}