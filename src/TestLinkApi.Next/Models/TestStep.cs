namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents a test step
/// </summary>
public record TestStep
{
    public int Id { get; init; }
    public int StepNumber { get; init; }
    public string Actions { get; init; } = string.Empty;
    public string ExpectedResults { get; init; } = string.Empty;
    public bool Active { get; init; }
    public int ExecutionType { get; init; }

    public TestStep() { }

    public TestStep(int stepNumber, string actions, string expectedResults, bool active = true, int executionType = 1)
    {
        StepNumber = stepNumber;
        Actions = actions;
        ExpectedResults = expectedResults;
        Active = active;
        ExecutionType = executionType;
    }
}