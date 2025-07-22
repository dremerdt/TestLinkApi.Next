namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents a TestLink test suite
/// </summary>
public record TestSuite
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
    public int ParentId { get; init; }
    public int NodeTypeId { get; init; }
    public int NodeOrder { get; init; }
}