namespace TestLinkApi.Next.Contracts;

using TestLinkApi.Next.Models;

/// <summary>
/// Interface for TestLink test suite operations
/// </summary>
public interface ITestSuiteOperations
{
    /// <summary>
    /// Create a new test suite
    /// </summary>
    /// <param name="testProjectId">ID of the test project</param>
    /// <param name="testSuiteName">Name of the test suite</param>
    /// <param name="details">Detailed description of the test suite</param>
    /// <param name="parentId">Optional parent test suite ID (for nested suites)</param>
    /// <param name="order">Optional display order within parent container</param>
    /// <param name="checkDuplicatedName">Whether to check for duplicate names (default: true)</param>
    /// <param name="actionOnDuplicatedName">Action to take if duplicate name found (default: "generate_new")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the create operation</returns>
    Task<GeneralResult> CreateTestSuiteAsync(
        int testProjectId,
        string testSuiteName,
        string details,
        int? parentId = null,
        int? order = null,
        bool checkDuplicatedName = true,
        string actionOnDuplicatedName = "generate_new",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload an attachment to a test suite
    /// </summary>
    /// <param name="testSuiteId">ID of the test suite</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="fileType">MIME type of the file</param>
    /// <param name="content">File content as byte array</param>
    /// <param name="title">Optional title for the attachment</param>
    /// <param name="description">Optional description for the attachment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the upload operation</returns>
    Task<AttachmentRequestResponse> UploadTestSuiteAttachmentAsync(
        int testSuiteId,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get test suites for a test plan
    /// </summary>
    /// <param name="testPlanId">ID of the test plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of test suites in the test plan</returns>
    Task<IEnumerable<TestSuite>> GetTestSuitesForTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get direct child test suites for a test suite
    /// </summary>
    /// <param name="testSuiteId">ID of the parent test suite</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of child test suites</returns>
    Task<IEnumerable<TestSuite>> GetTestSuitesForTestSuiteAsync(int testSuiteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a test suite by its ID
    /// </summary>
    /// <param name="testSuiteId">ID of the test suite</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test suite if found, null otherwise</returns>
    Task<TestSuite?> GetTestSuiteByIdAsync(int testSuiteId, CancellationToken cancellationToken = default);
}