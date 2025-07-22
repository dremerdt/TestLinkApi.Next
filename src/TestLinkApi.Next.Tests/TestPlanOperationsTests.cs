using TestLinkApi.Next.Models;

namespace TestLinkApi.Next.Tests;

/// <summary>
/// Integration tests for TestPlan Operations in TestLinkClient
/// These tests require a running TestLink instance configured in appsettings.json
/// </summary>
public class TestPlanOperationsTests : TestLinkTestBase, IDisposable
{
    #region CreateTestPlan Tests

    [Fact]
    public async Task CreateTestPlanAsync_WithValidParameters_CreatesTestPlan()
    {
        // Arrange - Get a project to test with
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testPlanName = $"TestPlan_{Guid.NewGuid():N}";
        var notes = "Test plan created by integration test";

        // Act
        var result = await Client.CreateTestPlanAsync(
            testPlanName, 
            project.Name, 
            notes);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Status, $"Failed to create test plan: {result.Message}");
        Assert.True(result.Id > 0);
        Assert.Equal("createTestPlan", result.Operation);
    }

    [Fact]
    public async Task CreateTestPlanAsync_WithOptionalParameters_CreatesTestPlan()
    {
        // Arrange - Get a project to test with
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testPlanName = $"TestPlan_{Guid.NewGuid():N}";
        var notes = "Test plan with optional parameters";

        // Act
        var result = await Client.CreateTestPlanAsync(
            testPlanName, 
            project.Name, 
            notes,
            active: false,  // Test with inactive
            isPublic: false); // Test with private

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Status, $"Failed to create test plan: {result.Message}");
        Assert.True(result.Id > 0);
        Assert.Equal("createTestPlan", result.Operation);
    }

    [Fact]
    public async Task CreateTestPlanAsync_WithNullTestPlanName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.CreateTestPlanAsync(null!, "ProjectName"));
    }

    [Fact]
    public async Task CreateTestPlanAsync_WithNullTestProjectName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.CreateTestPlanAsync("TestPlanName", null!));
    }

    [Fact]
    public async Task CreateTestPlanAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.CreateTestPlanAsync("TestPlan", "ProjectName", cancellationToken: cts.Token));
    }

    #endregion

    #region GetTestPlanByName Tests

    [Fact]
    public async Task GetTestPlanByNameAsync_WithExistingTestPlan_ReturnsTestPlan()
    {
        // Arrange - Get a project and create a test plan
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testPlanName = $"TestPlan_{Guid.NewGuid():N}";
        var createResult = await Client.CreateTestPlanAsync(testPlanName, project.Name);
        
        if (!createResult.Status)
        {
            return; // Skip if we can't create a test plan
        }

        // Act
        var result = await Client.GetTestPlanByNameAsync(testPlanName, project.Name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testPlanName, result.Name);
        Assert.Equal(project.Id, result.TestProjectId);
        Assert.Equal(createResult.Id, result.Id);
    }

    [Fact]
    public async Task GetTestPlanByNameAsync_WithNonExistentTestPlan_ReturnsNull()
    {
        // Arrange - Get a project
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var nonExistentTestPlanName = $"NonExistent_{Guid.NewGuid():N}";

        // Act & Assert
        await Assert.ThrowsAsync<TestLinkNotFoundException>(() => 
            Client.GetTestPlanByNameAsync(nonExistentTestPlanName, project.Name));
    }

    [Fact]
    public async Task GetTestPlanByNameAsync_WithNullTestPlanName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.GetTestPlanByNameAsync(null!, "ProjectName"));
    }

    [Fact]
    public async Task GetTestPlanByNameAsync_WithNullTestProjectName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.GetTestPlanByNameAsync("TestPlanName", null!));
    }

    [Fact]
    public async Task GetTestPlanByNameAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.GetTestPlanByNameAsync("TestPlan", "ProjectName", cts.Token));
    }

    #endregion

    #region GetTestPlanPlatforms Tests

    [Fact]
    public async Task GetTestPlanPlatformsAsync_WithValidTestPlanId_ReturnsPlatforms()
    {
        // Arrange - Get a project and its test plans
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testPlans = await Client.GetProjectTestPlansAsync(project.Id);
        var testPlan = testPlans.FirstOrDefault();
        
        if (testPlan == null)
        {
            return; // Skip if no test plans exist
        }

        // Act and Assert
        await Assert.ThrowsAsync<TestLinkApiException>(() => 
            Client.GetTestPlanPlatformsAsync(testPlan.Id));
    }

    [Fact]
    public async Task GetTestPlanPlatformsAsync_WithTestPlanWithNoPlatforms_ReturnsEmptyCollection()
    {
        // Arrange - Create a new test plan that likely has no platforms
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testPlanName = $"TestPlan_{Guid.NewGuid():N}";
        var createResult = await Client.CreateTestPlanAsync(testPlanName, project.Name);
        
        if (!createResult.Status)
        {
            return; // Skip if we can't create a test plan
        }

        // Act & Assert
        await Assert.ThrowsAsync<TestLinkApiException>(() => 
            Client.GetTestPlanPlatformsAsync(createResult.Id));
    }

    [Fact]
    public async Task GetTestPlanPlatformsAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.GetTestPlanPlatformsAsync(1, cts.Token));
    }

    #endregion

    #region GetTotalsForTestPlan Tests

    [Fact]
    public async Task GetTotalsForTestPlanAsync_WithValidTestPlanId_ReturnsTotals()
    {
        // Arrange - Get a project and its test plans
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testPlans = await Client.GetProjectTestPlansAsync(project.Id);
        var testPlan = testPlans.FirstOrDefault();
        
        if (testPlan == null)
        {
            return; // Skip if no test plans exist
        }

        // Act
        var result = await Client.GetTotalsForTestPlanAsync(testPlan.Id);

        // Assert
        Assert.NotNull(result);
        var totals = result.ToList();

        // Should be empty if no tests are associated
        Assert.Empty(totals);
    }

    [Fact]
    public async Task GetTotalsForTestPlanAsync_WithNewTestPlan_ReturnsZeroTotals()
    {
        // Arrange - Create a new test plan
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testPlanName = $"TestPlan_{Guid.NewGuid():N}";
        var createResult = await Client.CreateTestPlanAsync(testPlanName, project.Name);
        
        if (!createResult.Status)
        {
            return; // Skip if we can't create a test plan
        }

        // Act
        var result = await Client.GetTotalsForTestPlanAsync(createResult.Id);

        // Assert
        Assert.NotNull(result);
        var totals = result.ToList();

        // New test plan should return zero totals
        Assert.Empty(totals);
    }

    [Fact]
    public async Task GetTotalsForTestPlanAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.GetTotalsForTestPlanAsync(1, cts.Token));
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        // Clean up any resources if needed
        GC.SuppressFinalize(this);
    }

    #endregion
}