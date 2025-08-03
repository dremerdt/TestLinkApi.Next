namespace TestLinkApi.Next.Contracts;

/// <summary>
/// Combined interface for all TestLink operations
/// </summary>
public interface ITestLinkOperations : IBasicOperations, IProjectOperations, ITestSuiteOperations, ITestCaseOperations, ITestPlanOperations, IAttachmentOperations, IRequirementOperations
{
}