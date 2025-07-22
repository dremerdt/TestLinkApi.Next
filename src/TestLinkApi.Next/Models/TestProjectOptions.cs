namespace TestLinkApi.Next.Models;

/// <summary>
/// Configuration options for a TestLink test project
/// </summary>
public record TestProjectOptions
{
    /// <summary>
    /// Whether requirements feature is enabled for the project
    /// </summary>
    public bool RequirementsEnabled { get; init; } = true;

    /// <summary>
    /// Whether test priority feature is enabled for the project
    /// </summary>
    public bool TestPriorityEnabled { get; init; } = true;

    /// <summary>
    /// Whether automation feature is enabled for the project
    /// </summary>
    public bool AutomationEnabled { get; init; } = true;

    /// <summary>
    /// Whether inventory feature is enabled for the project
    /// </summary>
    public bool InventoryEnabled { get; init; } = true;
}