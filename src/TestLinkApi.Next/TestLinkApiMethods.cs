namespace TestLinkApi.Next;


/// <summary>
/// API method names
/// </summary>
public static class TestLinkApiMethods
{
    // Basic operations
    public const string Ping = "tl.ping";
    public const string SayHello = "tl.sayHello";
    public const string About = "tl.about";
    public const string CheckDevKey = "tl.checkDevKey";
    public const string DoesUserExist = "tl.doesUserExist";
    public const string SetTestMode = "tl.setTestMode";
    public const string Repeat = "tl.repeat";
    public const string GetFullPath = "tl.getFullPath";

    // Test Projects
    public const string GetProjects = "tl.getProjects";
    public const string GetProjectByName = "tl.getTestProjectByName";
    public const string CreateProject = "tl.createTestProject";
    public const string UploadProjectAttachment = "tl.uploadTestProjectAttachment";

    // Test Plans
    public const string GetProjectTestPlans = "tl.getProjectTestPlans";
    public const string GetTestPlanByName = "tl.getTestPlanByName";
    public const string CreateTestPlan = "tl.createTestPlan";
    public const string GetTestPlanPlatforms = "tl.getTestPlanPlatforms";
    public const string GetTotalsForTestPlan = "tl.getTotalsForTestPlan";

    // Test Suites
    public const string GetFirstLevelTestSuites = "tl.getFirstLevelTestSuitesForTestProject";
    public const string GetTestSuitesForTestPlan = "tl.getTestSuitesForTestPlan";
    public const string GetTestSuitesForTestSuite = "tl.getTestSuitesForTestSuite";
    public const string GetTestSuiteById = "tl.getTestSuiteByID";
    public const string CreateTestSuite = "tl.createTestSuite";
    public const string UploadTestSuiteAttachment = "tl.uploadTestSuiteAttachment";

    // Test Cases
    public const string GetTestCase = "tl.getTestCase";
    public const string GetTestCaseIdByName = "tl.getTestCaseIDByName";
    public const string GetTestCasesForTestSuite = "tl.getTestCasesForTestSuite";
    public const string GetTestCasesForTestPlan = "tl.getTestCasesForTestPlan";
    public const string CreateTestCase = "tl.createTestCase";
    public const string AddTestCaseToTestPlan = "tl.addTestCaseToTestPlan";
    public const string GetTestCaseAttachments = "tl.getTestCaseAttachments";
    public const string UploadTestCaseAttachment = "tl.uploadTestCaseAttachment";
    public const string GetTestCaseCustomFieldValue = "tl.getTestCaseCustomFieldDesignValue";

    // Builds
    public const string GetBuildsForTestPlan = "tl.getBuildsForTestPlan";
    public const string GetLatestBuildForTestPlan = "tl.getLatestBuildForTestPlan";
    public const string CreateBuild = "tl.createBuild";

    // Execution
    public const string ReportTestCaseResult = "tl.reportTCResult";
    public const string SetTestCaseExecutionResult = "tl.setTestCaseExecutionResult"; // Alias for reportTCResult
    public const string GetLastExecutionResult = "tl.getLastExecutionResult";
    public const string DeleteExecution = "tl.deleteExecution";

    // Requirements
    public const string AssignRequirements = "tl.assignRequirements";

    // Attachments
    public const string UploadAttachment = "tl.uploadAttachment";
    public const string UploadRequirementSpecificationAttachment = "tl.uploadRequirementSpecificationAttachment";
    public const string UploadRequirementAttachment = "tl.uploadRequirementAttachment";
}