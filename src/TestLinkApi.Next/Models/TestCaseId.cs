namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents a test case ID result from name lookup
/// </summary>
public record TestCaseId
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ParentId { get; init; }
    public int ExternalId { get; init; }
    public string TestSuiteName { get; init; } = string.Empty;
}