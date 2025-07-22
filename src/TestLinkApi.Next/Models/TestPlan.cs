namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents a TestLink test plan
/// </summary>
public record TestPlan
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public int TestProjectId { get; init; }
    public bool Active { get; init; }
    public bool IsOpen { get; init; }
    public bool IsPublic { get; init; }
}