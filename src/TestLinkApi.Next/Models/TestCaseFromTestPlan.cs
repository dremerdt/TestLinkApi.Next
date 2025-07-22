namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents a test case from a test plan context
/// </summary>
public record TestCaseFromTestPlan
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ExternalId { get; init; } = string.Empty;
    public string TestSuiteName { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ExecutionStatus { get; init; } = string.Empty;
    public bool Active { get; init; }
    public int Version { get; init; }
    public int TestCaseId { get; init; }
    public int TestCaseVersionId { get; init; }
    public int TestCaseVersionNumber { get; init; }
    public int TestSuiteId { get; init; }
    public int ExecutionType { get; init; }
    public int ExecutionOrder { get; init; }
    public int ExecutionId { get; init; }
    public int FeatureId { get; init; }
    public int PlatformId { get; init; }
    public string PlatformName { get; init; } = string.Empty;
    public int Importance { get; init; }
    public int Urgency { get; init; }
    public int Priority { get; init; }
    public int AssignerId { get; init; }
    public int UserId { get; init; }
    public int TesterId { get; init; }
    public int LinkedBy { get; init; }
    public DateTime LinkedTimestamp { get; init; }
    public int ExecutedOnTestPlan { get; init; }
    public int ExecutedOnBuild { get; init; }
    public int AssignedBuildId { get; init; }
    public int Executed { get; init; }
    public string ExecutionRunType { get; init; } = string.Empty;
    public string ExecutionTimestamp { get; init; } = string.Empty;
    public string ExecutionNotes { get; init; } = string.Empty;
    public int Z { get; init; }
}