namespace TestLinkApi.Next.Contracts;

using TestLinkApi.Next.Models;

/// <summary>
/// Interface for TestLink test plan operations
/// </summary>
public interface ITestPlanOperations
{
    /// <summary>
    /// Create a new test plan
    /// </summary>
    /// <param name="testPlanName">Name of the test plan</param>
    /// <param name="testProjectName">Name of the test project that contains the test plan</param>
    /// <param name="notes">Optional notes for the test plan</param>
    /// <param name="active">Whether the test plan is active (default: true)</param>
    /// <param name="isPublic">Whether the test plan is public (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the create operation</returns>
    Task<GeneralResult> CreateTestPlanAsync(
        string testPlanName,
        string testProjectName,
        string? notes = null,
        bool active = true,
        bool isPublic = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a test plan by its name
    /// </summary>
    /// <param name="testPlanName">Name of the test plan</param>
    /// <param name="testProjectName">Name of the test project that contains the test plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test plan if found, null otherwise</returns>
    Task<TestPlan?> GetTestPlanByNameAsync(
        string testPlanName, 
        string testProjectName, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all platforms associated with a test plan
    /// </summary>
    /// <param name="testPlanId">ID of the test plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of platforms associated with the test plan</returns>
    Task<IEnumerable<TestPlatform>> GetTestPlanPlatformsAsync(
        int testPlanId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get execution statistics/totals for a test plan grouped by platform
    /// </summary>
    /// <param name="testPlanId">ID of the test plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of test plan totals grouped by platform</returns>
    Task<IEnumerable<TestPlanTotal>> GetTotalsForTestPlanAsync(
        int testPlanId, 
        CancellationToken cancellationToken = default);
}