namespace TestLinkApi.Next;

/// <summary>
/// Constants used by the TestLink API
/// </summary>
public static class TestLinkConstants
{
    /// <summary>
    /// Test case execution types
    /// </summary>
    public static class ExecutionType
    {
        public const int Manual = 1;
        public const int Automated = 2;
    }

    /// <summary>
    /// Test case importance levels
    /// </summary>
    public static class Importance
    {
        public const int Low = 1;
        public const int Medium = 2;
        public const int High = 3;
    }

    /// <summary>
    /// Test execution statuses
    /// </summary>
    public static class ExecutionStatus
    {
        public const string Pass = "p";
        public const string Fail = "f";
        public const string Blocked = "b";
        public const string NotRun = "n";
    }

    /// <summary>
    /// Actions to take when duplicate names are found
    /// </summary>
    public static class ActionOnDuplicatedName
    {
        public const string Block = "block";
        public const string GenerateNew = "generate_new";
        public const string CreateNewVersion = "create_new_version";
    }

    /// <summary>
    /// Default order for test cases and test suites
    /// </summary>
    public const int DefaultOrder = 0;

    /// <summary>
    /// Node types in TestLink hierarchy
    /// </summary>
    public static class NodeType
    {
        public const int TestProject = 1;
        public const int TestSuite = 2;
        public const int TestCase = 3;
        public const int TestCaseVersion = 4;
        public const int TestPlan = 5;
        public const int Build = 6;
        public const int Platform = 7;
    }

    /// <summary>
    /// Common error codes returned by TestLink API
    /// </summary>
    public static class ErrorCodes
    {
        public const int NoDevKey = 1000;
        public const int InvalidAuth = 2000;
        public const int InsufficientRights = 3000;
        public const int NoTestCaseId = 5000;
        public const int InvalidTestCaseId = 5040;
        public const int NoTestCaseByName = 5030;
        public const int NoTestPlanId = 6000;
        public const int InvalidTestPlanId = 6040;
        public const int NoTestProjectId = 7000;
        public const int InvalidTestProjectId = 7040;
        public const int ProjectIsEmpty = 7008;
        public const int NoTestSuiteId = 8000;
        public const int InvalidTestSuiteId = 8040;
        public const int UserNotFound = 10000;
    }
}