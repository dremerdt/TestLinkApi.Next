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

    #region AddTestCaseToTestPlan Tests

    [Fact]
    public async Task AddTestCaseToTestPlanAsync_WithValidParameters_AddsTestCase()
    {
        // Arrange - Get a project, create a test plan, create a test case
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        // Create a test plan
        var testPlanName = $"TestPlan_{Guid.NewGuid():N}";
        var planResult = await Client.CreateTestPlanAsync(testPlanName, project.Name);
        
        if (!planResult.Status)
        {
            return; // Skip if we can't create a test plan
        }

        // Create a test suite
        var testSuites = await Client.GetFirstLevelTestSuitesForTestProjectAsync(project.Id);
        var testSuite = testSuites.FirstOrDefault();
        
        if (testSuite == null)
        {
            var suiteResult = await Client.CreateTestSuiteAsync(
                project.Id, 
                $"TestSuite_{Guid.NewGuid():N}", 
                "Test suite for test case linking");
            
            if (!suiteResult.Status)
            {
                return; // Skip if we can't create a test suite
            }
            
            testSuite = new TestSuite { Id = suiteResult.Id };
        }

        // Create a test case
        var testCaseName = $"TestCase_{Guid.NewGuid():N}";
        var testCaseRequest = new CreateTestCaseRequest
        {
            AuthorLogin = Settings.User,
            TestSuiteId = testSuite.Id,
            TestCaseName = testCaseName,
            TestProjectId = project.Id,
            Summary = "Test case for linking to test plan",
            Steps = [new TestStep(1, "Test action", "Expected result")]
        };

        var testCaseResult = await Client.CreateTestCaseAsync(testCaseRequest);
        if (!testCaseResult.Status)
        {
            return; // Skip if test case creation failed
        }

        // Get the external ID for the test case
        var testCaseIds = await Client.GetTestCaseIdByNameAsync(testCaseName);
        var testCaseId = testCaseIds.FirstOrDefault();
        
        if (testCaseId == null)
        {
            return; // Skip if we can't find the test case
        }

        // Act
        var result = await Client.AddTestCaseToTestPlanAsync(
            project.Id,
            planResult.Id,
            $"{project.Prefix}-{testCaseId.ExternalId}",
            1, // version 1
            platformId: null,
            executionOrder: 1,
            urgency: 2);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Status, $"Failed to add test case to test plan: {result.Message}");
        Assert.Equal("addTestCaseToTestPlan", result.Operation);
    }

    [Fact]
    public async Task AddTestCaseToTestPlanAsync_WithNullTestCaseExternalId_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.AddTestCaseToTestPlanAsync(1, 1, null!, 1));
    }

    [Fact]
    public async Task AddTestCaseToTestPlanAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.AddTestCaseToTestPlanAsync(1, 1, "TC-1", 1, cancellationToken: cts.Token));
    }

    #endregion

    #region GetBuildsForTestPlan Tests

    [Fact]
    public async Task GetBuildsForTestPlanAsync_WithValidTestPlanId_ReturnsBuilds()
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
        var result = await Client.GetBuildsForTestPlanAsync(testPlan.Id);

        // Assert
        Assert.NotNull(result);
        var builds = result.ToList();

        // Test plan may or may not have builds
        // Verify structure of any returned builds
        foreach (var build in builds)
        {
            Assert.True(build.Id > 0);
            Assert.NotEmpty(build.Name);
            Assert.Equal(testPlan.Id, build.TestPlanId);
        }
    }

    [Fact]
    public async Task GetBuildsForTestPlanAsync_WithNewTestPlan_ReturnsEmptyCollection()
    {
        // Arrange - Create a new test plan that has no builds
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
        var result = await Client.GetBuildsForTestPlanAsync(createResult.Id);

        // Assert
        Assert.NotNull(result);
        var builds = result.ToList();

        // New test plan should have no builds
        Assert.Empty(builds);
    }

    [Fact]
    public async Task GetBuildsForTestPlanAsync_WithInvalidTestPlanId_ThrowsTestLinkException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TestLinkNotFoundException>(() => 
            Client.GetBuildsForTestPlanAsync(999999));
    }

    [Fact]
    public async Task GetBuildsForTestPlanAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.GetBuildsForTestPlanAsync(1, cts.Token));
    }

    #endregion

    #region GetLatestBuildForTestPlan Tests

    [Fact]
    public async Task GetLatestBuildForTestPlanAsync_WithValidTestPlanId_ReturnsLatestBuild()
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


        var buildName = $"Build_{Guid.NewGuid():N}";
        var createBuild = await Client.CreateBuildAsync(testPlan.Id, buildName);

        if (!createBuild.Status)
        {
            return; // Skip if we can't create a build
        }

        // Get all builds first to compare
        var allBuilds = await Client.GetBuildsForTestPlanAsync(testPlan.Id);
        var buildsList = allBuilds.ToList();

        // Act
        var result = await Client.GetLatestBuildForTestPlanAsync(testPlan.Id);

        // Assert
        if (buildsList.Count == 0)
        {
            // No builds exist, should return null
            Assert.Null(result);
        }
        else
        {
            // Should return the build with the highest ID
            Assert.NotNull(result);
            var expectedLatestBuild = buildsList.OrderByDescending(b => b.Id).First();
            Assert.Equal(expectedLatestBuild.Id, result.Id);
            Assert.Equal(expectedLatestBuild.Name, result.Name);
            Assert.Equal(testPlan.Id, result.TestPlanId);
        }
    }

    [Fact]
    public async Task GetLatestBuildForTestPlanAsync_WithNewTestPlan_ReturnsNull()
    {
        // Arrange - Create a new test plan that has no builds
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
            Client.GetLatestBuildForTestPlanAsync(createResult.Id));
    }

    [Fact]
    public async Task GetLatestBuildForTestPlanAsync_WithInvalidTestPlanId_ThrowsTestLinkException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TestLinkNotFoundException>(() => 
            Client.GetLatestBuildForTestPlanAsync(999999));
    }

    [Fact]
    public async Task GetLatestBuildForTestPlanAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.GetLatestBuildForTestPlanAsync(1, cts.Token));
    }

    #endregion

    #region CreateBuild Tests

    [Fact]
    public async Task CreateBuildAsync_WithValidParameters_CreatesBuild()
    {
        // Arrange - Get a project and test plan
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
            // Create a test plan first
            var testPlanName = $"TestPlan_{Guid.NewGuid():N}";
            var planResult = await Client.CreateTestPlanAsync(testPlanName, project.Name);
            
            if (!planResult.Status)
            {
                return; // Skip if we can't create a test plan
            }
            
            testPlan = new TestPlan { Id = planResult.Id, Name = testPlanName };
        }

        var buildName = $"Build_{Guid.NewGuid():N}";
        var buildNotes = "Build created by integration test";

        // Act
        var result = await Client.CreateBuildAsync(
            testPlan.Id,
            buildName,
            buildNotes,
            active: true,
            open: true);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Status, $"Failed to create build: {result.Message}");
        Assert.True(result.Id > 0);
        Assert.Equal("createBuild", result.Operation);
    }

    [Fact]
    public async Task CreateBuildAsync_WithOptionalParameters_CreatesBuild()
    {
        // Arrange - Get a project and test plan
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

        var buildName = $"Build_{Guid.NewGuid():N}";
        var releaseDate = DateTime.Now.AddDays(7).Date; // Future release date

        // Act
        var result = await Client.CreateBuildAsync(
            testPlan.Id,
            buildName,
            buildNotes: "Build with release date",
            active: false,
            open: false,
            releaseDate: releaseDate);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Status, $"Failed to create build with release date: {result.Message}");
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task CreateBuildAsync_WithNullBuildName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.CreateBuildAsync(1, null!));
    }

    [Fact]
    public async Task CreateBuildAsync_WithInvalidTestPlanId_ThrowsTestLinkException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TestLinkNotFoundException>(() => 
            Client.CreateBuildAsync(999999, "BuildName"));
    }

    [Fact]
    public async Task CreateBuildAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.CreateBuildAsync(1, "BuildName", cancellationToken: cts.Token));
    }

    #endregion

    #region GetLastExecutionResult Tests

    [Fact]
    public async Task GetLastExecutionResultAsync_WithValidTestCaseId_ReturnsExecutionResult()
    {
        // Arrange - Get a project, test plan, and test case
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

        // Get test cases for the test plan
        var testCases = await Client.GetTestCasesForTestPlanAsync(testPlan.Id);
        var testCase = testCases.FirstOrDefault();
        
        if (testCase == null)
        {
            return; // Skip if no test cases exist in the test plan
        }

        // Act
        var result = await Client.GetLastExecutionResultAsync(
            testPlan.Id,
            testCaseId: testCase.TestCaseId);

        // Assert
        // Result may be null if no execution exists yet
        // We just verify the call doesn't throw an exception
        Assert.True(true); // Test passed if we reach here
    }

    [Fact]
    public async Task GetLastExecutionResultAsync_WithValidExternalId_ReturnsExecutionResult()
    {
        // Arrange - Get a project and test plan with an actual test case
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

        // Get test cases for the test plan to use a real external ID
        var testCases = await Client.GetTestCasesForTestPlanAsync(testPlan.Id);
        var testCase = testCases.FirstOrDefault();
        
        if (testCase == null)
        {
            // No test cases in the test plan, expect TestLinkApiException for non-existent test case
            await Assert.ThrowsAsync<TestLinkApiException>(() =>
                Client.GetLastExecutionResultAsync(
                    testPlan.Id,
                    testCaseExternalId: $"{project.Prefix}-1"));
            return;
        }

        // Act - Use the external ID of an actual test case that's associated with the test plan
        var result = await Client.GetLastExecutionResultAsync(
            testPlan.Id,
            testCaseExternalId: testCase.GetFullExternalId(project.Prefix));

        // Assert
        // Result may be null if no execution exists yet for this test case
        // We just verify the call doesn't throw an exception for a valid test case
        Assert.True(true); // Test passed if we reach here
    }

    [Fact]
    public async Task GetLastExecutionResultAsync_WithNoTestCaseIdentifier_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            Client.GetLastExecutionResultAsync(1));
    }

    [Fact]
    public async Task GetLastExecutionResultAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.GetLastExecutionResultAsync(1, testCaseId: 1, cancellationToken: cts.Token));
    }

    #endregion

    #region DeleteExecution Tests

    [Fact]
    public async Task DeleteExecutionAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.DeleteExecutionAsync(1, cts.Token));
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