namespace TestLinkApi.Next.Models;

/// <summary>
/// Request model for creating a test case
/// </summary>
public record CreateTestCaseRequest
{
    public required string AuthorLogin { get; init; }
    public required int TestSuiteId { get; init; }
    public required string TestCaseName { get; init; }
    public required int TestProjectId { get; init; }
    public required string Summary { get; init; }
    public TestStep[] Steps { get; init; } = [];
    public string? Preconditions { get; init; }
    public string? Keywords { get; init; }
    public int Order { get; init; } = 0;
    public bool CheckDuplicatedName { get; init; } = true;
    public string ActionOnDuplicatedName { get; init; } = "block";
    public int ExecutionType { get; init; } = 1; // 1=manual, 2=automated
    public int Importance { get; init; } = 2; // 1=low, 2=medium, 3=high
}

/// <summary>
/// Request model for adding a test case to a test plan
/// </summary>
public record AddTestCaseToTestPlanRequest
{
    /// <summary>
    /// Test project ID
    /// </summary>
    public required int TestProjectId { get; init; }

    /// <summary>
    /// Test plan ID
    /// </summary>
    public required int TestPlanId { get; init; }

    /// <summary>
    /// Test case external ID (e.g., "PROJ-123")
    /// </summary>
    public required string TestCaseExternalId { get; init; }

    /// <summary>
    /// Version number of the test case to link
    /// </summary>
    public required int Version { get; init; }

    /// <summary>
    /// Platform ID (optional, only if test plan has platforms)
    /// </summary>
    public int? PlatformId { get; init; }

    /// <summary>
    /// Execution order (optional)
    /// </summary>
    public int? ExecutionOrder { get; init; }

    /// <summary>
    /// Urgency level (optional)
    /// </summary>
    public int? Urgency { get; init; }
}