using Microsoft.Extensions.DependencyInjection;

namespace TestLinkApi.Next;

/// <summary>
/// Extension methods for dependency injection and convenience
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds TestLink API client to the service collection
    /// </summary>
    public static IServiceCollection AddTestLinkClient(this IServiceCollection services, string apiKey, string baseUrl)
    {
        services.AddHttpClient<TestLinkClient>();
        services.AddSingleton<TestLinkClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(TestLinkClient));
            return new TestLinkClient(apiKey, baseUrl, httpClient);
        });

        return services;
    }

    /// <summary>
    /// Adds TestLink API client to the service collection with configuration
    /// </summary>
    public static IServiceCollection AddTestLinkClient(this IServiceCollection services, Action<TestLinkClientBuilder> configure)
    {
        var builder = TestLinkClientBuilder.Create();
        configure(builder);

        services.AddHttpClient<TestLinkClient>();
        services.AddSingleton<TestLinkClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(TestLinkClient));
            
            // Create a new builder and apply configuration
            var newBuilder = TestLinkClientBuilder.Create();
            configure(newBuilder);
            
            // Override with the DI HttpClient
            return newBuilder.WithHttpClient(httpClient).Build();
        });

        return services;
    }

    /// <summary>
    /// Extension method to check if a test case execution status indicates success
    /// </summary>
    public static bool IsSuccess(this string status)
    {
        return string.Equals(status, TestLinkConstants.ExecutionStatus.Pass, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, "p", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extension method to check if a test case execution status indicates failure
    /// </summary>
    public static bool IsFailure(this string status)
    {
        return string.Equals(status, TestLinkConstants.ExecutionStatus.Fail, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, "f", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extension method to check if a test case execution status indicates blocked
    /// </summary>
    public static bool IsBlocked(this string status)
    {
        return string.Equals(status, TestLinkConstants.ExecutionStatus.Blocked, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, "b", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extension method to check if a test case execution status indicates not run
    /// </summary>
    public static bool IsNotRun(this string status)
    {
        return string.Equals(status, TestLinkConstants.ExecutionStatus.NotRun, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, "n", StringComparison.OrdinalIgnoreCase) ||
               string.IsNullOrEmpty(status);
    }

    /// <summary>
    /// Converts an execution status to a human-readable string
    /// </summary>
    public static string ToDisplayString(this string status)
    {
        return status.ToLowerInvariant() switch
        {
            "p" => "Passed",
            "f" => "Failed",
            "b" => "Blocked",
            "n" or "" => "Not Run",
            _ => status
        };
    }

    /// <summary>
    /// Gets the full external ID for a test case (with project prefix)
    /// </summary>
    public static string GetFullExternalId(this TestCase testCase, string projectPrefix)
    {
        return $"{projectPrefix}-{testCase.ExternalId}";
    }

    /// <summary>
    /// Gets the full external ID for a test case (with project prefix)
    /// Note: TestCaseFromTestSuite does not include external ID - use GetTestCase API for full details
    /// </summary>
    public static string GetFullExternalId(this TestCaseFromTestSuite testCase, string projectPrefix)
    {
        // TestCaseFromTestSuite from GetTestCasesForTestSuite API doesn't include external ID
        // This is a simplified response that only contains basic node information
        throw new InvalidOperationException("TestCaseFromTestSuite does not contain external ID. Use GetTestCase API to get test case details with external ID.");
    }

    /// <summary>
    /// Gets the full external ID for a test case (with project prefix)
    /// </summary>
    public static string GetFullExternalId(this TestCaseFromTestPlan testCase, string projectPrefix)
    {
        return $"{projectPrefix}-{testCase.ExternalId}";
    }

    /// <summary>
    /// Creates a test step with the specified parameters
    /// </summary>
    public static TestStep CreateStep(int stepNumber, string actions, string expectedResults, bool active = true, int executionType = TestLinkConstants.ExecutionType.Manual)
    {
        return new TestStep(stepNumber, actions, expectedResults, active, executionType);
    }
}