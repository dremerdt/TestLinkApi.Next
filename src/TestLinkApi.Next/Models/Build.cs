namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents a build
/// </summary>
public record Build
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public int TestPlanId { get; init; }
    public bool Active { get; init; }
    public bool IsOpen { get; init; }
}