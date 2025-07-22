namespace TestLinkApi.Next.Contracts;

using TestLinkApi.Next.Models;

/// <summary>
/// Interface for TestLink test case operations
/// </summary>
public interface ITestCaseOperations
{
    /// <summary>
    /// Create a new test case
    /// </summary>
    /// <param name="request">Test case creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the create operation</returns>
    Task<GeneralResult> CreateTestCaseAsync(
        CreateTestCaseRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload an attachment to a test case
    /// </summary>
    /// <param name="testCaseId">ID of the test case</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="fileType">MIME type of the file</param>
    /// <param name="content">File content as byte array</param>
    /// <param name="title">Optional title for the attachment</param>
    /// <param name="description">Optional description for the attachment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the upload operation</returns>
    Task<AttachmentRequestResponse> UploadTestCaseAttachmentAsync(
        int testCaseId,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get test cases contained in a test suite
    /// </summary>
    /// <param name="testSuiteId">ID of the test suite</param>
    /// <param name="deep">Whether to include test cases from child test suites (default: true)</param>
    /// <param name="details">Level of detail to return ("simple", "full") (default: "simple")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of test cases in the test suite</returns>
    Task<IEnumerable<TestCaseFromTestSuite>> GetTestCasesForTestSuiteAsync(
        int testSuiteId,
        bool deep = true,
        string details = "simple",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get test cases linked to a test plan
    /// </summary>
    /// <param name="testPlanId">ID of the test plan</param>
    /// <param name="testCaseId">Optional specific test case ID to filter by</param>
    /// <param name="buildId">Optional build ID to filter by</param>
    /// <param name="keywordId">Optional keyword ID to filter by</param>
    /// <param name="keywords">Optional keyword names to filter by</param>
    /// <param name="executed">Optional filter for executed test cases only</param>
    /// <param name="assignedTo">Optional user ID filter for assigned test cases</param>
    /// <param name="executeStatus">Optional execution status to filter by</param>
    /// <param name="executionType">Optional execution type to filter by</param>
    /// <param name="getStepsInfo">Whether to include test steps information (default: false)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of test cases in the test plan</returns>
    Task<IEnumerable<TestCaseFromTestPlan>> GetTestCasesForTestPlanAsync(
        int testPlanId,
        int? testCaseId = null,
        int? buildId = null,
        int? keywordId = null,
        string? keywords = null,
        bool? executed = null,
        int? assignedTo = null,
        string? executeStatus = null,
        int? executionType = null,
        bool getStepsInfo = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find a test case by its name
    /// </summary>
    /// <param name="testCaseName">Name of the test case (case sensitive)</param>
    /// <param name="testSuiteName">Optional test suite name to narrow search</param>
    /// <param name="testProjectName">Optional test project name to narrow search</param>
    /// <param name="testCasePathName">Optional full test case path name (alternative to other parameters)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test case ID information if found</returns>
    Task<IEnumerable<TestCaseId>> GetTestCaseIdByNameAsync(
        string testCaseName,
        string? testSuiteName = null,
        string? testProjectName = null,
        string? testCasePathName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get value of a custom field with scope 'design' for a given test case
    /// </summary>
    /// <param name="testCaseExternalId">External ID of the test case</param>
    /// <param name="versionNumber">Version number of the test case</param>
    /// <param name="testProjectId">ID of the test project</param>
    /// <param name="customFieldName">Name of the custom field</param>
    /// <param name="details">Level of detail to return ("value", "simple", "full") (default: "value")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Custom field value or detailed information</returns>
    Task<object> GetTestCaseCustomFieldDesignValueAsync(
        string testCaseExternalId,
        int versionNumber,
        int testProjectId,
        string customFieldName,
        string details = "value",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get attachments for a test case
    /// </summary>
    /// <param name="testCaseId">Internal ID of the test case</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of attachments for the test case</returns>
    Task<IEnumerable<Attachment>> GetTestCaseAttachmentsAsync(
        int testCaseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a test case by its internal or external ID
    /// </summary>
    /// <param name="testCaseId">Internal ID of the test case</param>
    /// <param name="versionNumber">Optional version number (latest version if not specified)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test case information if found</returns>
    Task<TestCase?> GetTestCaseAsync(
        int testCaseId,
        int? versionNumber = null,
        CancellationToken cancellationToken = default);
}