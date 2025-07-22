namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents a test platform
/// </summary>
public record Platform
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}