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

    /// <summary>
    /// Add a test case version to a test plan
    /// </summary>
    /// <param name="testProjectId">Test project ID</param>
    /// <param name="testPlanId">Test plan ID</param>
    /// <param name="testCaseExternalId">Test case external ID (e.g., "PROJ-123")</param>
    /// <param name="version">Version number of the test case to link</param>
    /// <param name="platformId">Platform ID (optional, only if test plan has platforms)</param>
    /// <param name="executionOrder">Execution order (optional)</param>
    /// <param name="urgency">Urgency level (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the add operation</returns>
    Task<GeneralResult> AddTestCaseToTestPlanAsync(
        int testProjectId,
        int testPlanId,
        string testCaseExternalId,
        int version,
        int? platformId = null,
        int? executionOrder = null,
        int? urgency = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all builds for a test plan
    /// </summary>
    /// <param name="testPlanId">ID of the test plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of builds for the test plan</returns>
    Task<IEnumerable<Build>> GetBuildsForTestPlanAsync(
        int testPlanId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the latest build for a test plan (build with highest ID)
    /// </summary>
    /// <param name="testPlanId">ID of the test plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest build for the test plan, or null if no builds exist</returns>
    Task<Build?> GetLatestBuildForTestPlanAsync(
        int testPlanId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new build for a test plan
    /// </summary>
    /// <param name="testPlanId">ID of the test plan</param>
    /// <param name="buildName">Name of the build</param>
    /// <param name="buildNotes">Optional notes for the build</param>
    /// <param name="active">Whether the build is active (default: true)</param>
    /// <param name="open">Whether the build is open (default: true)</param>
    /// <param name="releaseDate">Optional release date for the build</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the create operation</returns>
    Task<GeneralResult> CreateBuildAsync(
        int testPlanId,
        string buildName,
        string? buildNotes = null,
        bool active = true,
        bool open = true,
        DateTime? releaseDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the last execution result for a test case
    /// </summary>
    /// <param name="testPlanId">ID of the test plan</param>
    /// <param name="testCaseId">ID of the test case</param>
    /// <param name="testCaseExternalId">External ID of the test case (alternative to testCaseId)</param>
    /// <param name="platformId">Optional platform ID</param>
    /// <param name="platformName">Optional platform name</param>
    /// <param name="buildId">Optional build ID</param>
    /// <param name="buildName">Optional build name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Last execution result for the test case, or null if no execution exists</returns>
    Task<ExecutionResult?> GetLastExecutionResultAsync(
        int testPlanId,
        int? testCaseId = null,
        string? testCaseExternalId = null,
        int? platformId = null,
        string? platformName = null,
        int? buildId = null,
        string? buildName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a test case execution
    /// </summary>
    /// <param name="executionId">ID of the execution to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the delete operation</returns>
    Task<GeneralResult> DeleteExecutionAsync(
        int executionId,
        CancellationToken cancellationToken = default);
}