namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents a test case from a test suite context (simplified response from GetTestCasesForTestSuite)
/// </summary>
public record TestCaseFromTestSuite
{
    /// <summary>
    /// The test case version ID
    /// </summary>
    public int Id { get; init; }
    
    /// <summary>
    /// The test case name
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// The parent test suite ID
    /// </summary>
    public int ParentId { get; init; }
    
    /// <summary>
    /// Node type ID (3 = test case)
    /// </summary>
    public int NodeTypeId { get; init; }
    
    /// <summary>
    /// Display order within the test suite
    /// </summary>
    public int NodeOrder { get; init; }
    
    /// <summary>
    /// Node table name ("testcases")
    /// </summary>
    public string NodeTable { get; init; } = string.Empty;
}