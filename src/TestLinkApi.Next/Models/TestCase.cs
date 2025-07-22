namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents a TestLink test case
/// </summary>
public record TestCase
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ExternalId { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Preconditions { get; init; } = string.Empty;
    public int Version { get; init; }
    public int TestSuiteId { get; init; }
    public int TestCaseId { get; init; }
    public bool Active { get; init; }
    public bool IsOpen { get; init; }
    public int Status { get; init; }
    public int Importance { get; init; }
    public int ExecutionType { get; init; }
    public int AuthorId { get; init; }
    public int UpdaterId { get; init; }
    public string AuthorLogin { get; init; } = string.Empty;
    public string UpdaterLogin { get; init; } = string.Empty;
    public string AuthorFirstName { get; init; } = string.Empty;
    public string AuthorLastName { get; init; } = string.Empty;
    public string UpdaterFirstName { get; init; } = string.Empty;
    public string UpdaterLastName { get; init; } = string.Empty;
    public DateTime CreationTimestamp { get; init; }
    public DateTime ModificationTimestamp { get; init; }
    public string Layout { get; init; } = string.Empty;
    public int NodeOrder { get; init; }
    public List<TestStep> Steps { get; init; } = new();
}