namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents an execution result
/// </summary>
public record ExecutionResult
{
    public int Id { get; init; }
    public string Notes { get; init; } = string.Empty;
    public DateTime ExecutionTimestamp { get; init; }
    public int ExecutionType { get; init; }
    public int BuildId { get; init; }
    public int TestCaseVersionId { get; init; }
    public int TestCaseVersionNumber { get; init; }
    public string Status { get; init; } = string.Empty;
    public int TestPlanId { get; init; }
    public int TesterId { get; init; }
}