namespace TestLinkApi.Next.Contracts;

/// <summary>
/// Interface for TestLink basic operations
/// </summary>
public interface IBasicOperations
{
    /// <summary>
    /// Ping the TestLink server to check connectivity
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Server response</returns>
    Task<string> PingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Say hello to the TestLink server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Server response with greeting</returns>
    Task<string> SayHelloAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get information about the TestLink server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Server version and information</returns>
    Task<string> AboutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the developer key is valid
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<string> CheckDevKeyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a user exists in the TestLink system
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists, false otherwise</returns>
    Task<bool> DoesUserExistAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set test mode on or off
    /// </summary>
    /// <param name="set">True to enable test mode, false to disable</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> SetTestMode(bool set, CancellationToken cancellationToken = default);

    /// <summary>
    /// Repeat the provided text
    /// </summary>
    /// <param name="text">Text to repeat</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repeated text</returns>
    Task<string> RepeatAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the full path for a node by its ID
    /// </summary>
    /// <param name="nodeId">Node ID to get path for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Full path string</returns>
    Task<string> GetFullPathAsync(int nodeId, CancellationToken cancellationToken = default);
}