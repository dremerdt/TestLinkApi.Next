namespace TestLinkApi.Next.Contracts;

using TestLinkApi.Next.Models;

/// <summary>
/// Interface for TestLink requirement operations
/// </summary>
public interface IRequirementOperations
{
    /// <summary>
    /// Assign requirements to a test case
    /// </summary>
    /// <param name="request">Request containing assignment information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the assignment operation</returns>
    Task<GeneralResult> AssignRequirementsAsync(
        AssignRequirementsRequest request,
        CancellationToken cancellationToken = default);
}