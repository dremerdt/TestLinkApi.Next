namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents a TestLink test project
/// </summary>
public record TestProject
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Prefix { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public bool Active { get; init; }
    public bool RequirementsEnabled { get; init; }
    public bool TestPriorityEnabled { get; init; }
    public bool AutomationEnabled { get; init; }
    public bool InventoryEnabled { get; init; }
    public int TestCaseCounter { get; init; }
    public bool IsPublic { get; init; }
    public bool IssueTrackerEnabled { get; init; }
    public bool ReqMgrIntegrationEnabled { get; init; }
    public string ApiKey { get; init; } = string.Empty;
}