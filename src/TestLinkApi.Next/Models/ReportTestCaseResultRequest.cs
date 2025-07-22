namespace TestLinkApi.Next.Models;

/// <summary>
/// Request model for reporting test case results
/// </summary>
public record ReportTestCaseResultRequest
{
    public required int TestCaseId { get; init; }
    public required int TestPlanId { get; init; }
    public required string Status { get; init; } // "p"=pass, "f"=fail, "b"=blocked
    public int? PlatformId { get; init; }
    public string? PlatformName { get; init; }
    public bool Overwrite { get; init; } = false;
    public bool Guess { get; init; } = true;
    public string? Notes { get; init; }
    public int? BuildId { get; init; }
    public int? BugId { get; init; }
}