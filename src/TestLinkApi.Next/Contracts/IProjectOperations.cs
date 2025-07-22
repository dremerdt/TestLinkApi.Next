namespace TestLinkApi.Next.Contracts;

using TestLinkApi.Next.Models;

/// <summary>
/// Interface for TestLink project operations
/// </summary>
public interface IProjectOperations
{
    /// <summary>
    /// Get all projects from the TestLink server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of test projects</returns>
    Task<IEnumerable<TestProject>> GetProjectsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a test project by its name
    /// </summary>
    /// <param name="testProjectName">Name of the test project</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test project if found, null otherwise</returns>
    Task<TestProject?> GetTestProjectByNameAsync(string testProjectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new test project
    /// </summary>
    /// <param name="testProjectName">Name of the test project</param>
    /// <param name="testCasePrefix">Prefix for test case identifiers</param>
    /// <param name="notes">Optional notes for the project</param>
    /// <param name="options">Optional project configuration options</param>
    /// <param name="active">Whether the project is active (default: true)</param>
    /// <param name="isPublic">Whether the project is public (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the create operation</returns>
    Task<GeneralResult> CreateTestProjectAsync(
        string testProjectName,
        string testCasePrefix,
        string? notes = null,
        TestProjectOptions? options = null,
        bool active = true,
        bool isPublic = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload an attachment to a test project
    /// </summary>
    /// <param name="testProjectId">ID of the test project</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="fileType">MIME type of the file</param>
    /// <param name="content">File content as byte array</param>
    /// <param name="title">Optional title for the attachment</param>
    /// <param name="description">Optional description for the attachment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the upload operation</returns>
    Task<AttachmentRequestResponse> UploadTestProjectAttachmentAsync(
        int testProjectId,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all test plans for a test project
    /// </summary>
    /// <param name="testProjectId">ID of the test project</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of test plans for the project</returns>
    Task<IEnumerable<TestPlan>> GetProjectTestPlansAsync(int testProjectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get first level test suites for a test project (top-level test suites)
    /// </summary>
    /// <param name="testProjectId">ID of the test project</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of first level test suites</returns>
    Task<IEnumerable<TestSuite>> GetFirstLevelTestSuitesForTestProjectAsync(int testProjectId, CancellationToken cancellationToken = default);
}