namespace TestLinkApi.Next.Models;

/// <summary>
/// Request model for assigning requirements to test cases
/// </summary>
public record AssignRequirementsRequest
{
    /// <summary>
    /// Test project ID
    /// </summary>
    public required int TestProjectId { get; init; }
    
    /// <summary>
    /// Test case external ID (e.g., "PROJ-123")
    /// </summary>
    public required string TestCaseExternalId { get; init; }
    
    /// <summary>
    /// Array of requirement IDs to assign
    /// </summary>
    public required int[] RequirementIds { get; init; }
}