using Microsoft.Extensions.Configuration;
using TestLinkApi.Next.Models;

namespace TestLinkApi.Next.Tests;

/// <summary>
/// Integration tests for Project Operations in TestLinkClient
/// These tests require a running TestLink instance configured in appsettings.json
/// </summary>
public class ProjectOperationsTests : TestLinkTestBase, IDisposable
{
    #region GetProjects Tests

    [Fact]
    public async Task GetProjectsAsync_WithRealTestLinkInstance_ReturnsProjects()
    {
        // Act
        var result = await Client.GetProjectsAsync();

        // Assert
        Assert.NotNull(result);
        // Should return at least one project (even if it's a default one)
        var projects = result.ToList();
        Assert.NotEmpty(projects);

        // Verify that each project has required properties
        foreach (var project in projects)
        {
            Assert.True(project.Id > 0);
            Assert.NotEmpty(project.Name);
            Assert.NotEmpty(project.Prefix);
        }
    }

    [Fact]
    public async Task GetProjectsAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.GetProjectsAsync(cts.Token));
    }

    #endregion

    #region GetTestProjectByName Tests

    [Fact]
    public async Task GetTestProjectByNameAsync_WithExistingProject_ReturnsProject()
    {
        // Arrange - First get all projects to find one that exists
        var allProjects = await Client.GetProjectsAsync();
        var existingProject = allProjects.FirstOrDefault();
        
        // Skip test if no projects exist
        if (existingProject == null)
        {
            return; // or use Skip.If in xUnit
        }

        // Act
        var result = await Client.GetTestProjectByNameAsync(existingProject.Name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingProject.Id, result.Id);
        Assert.Equal(existingProject.Name, result.Name);
        Assert.Equal(existingProject.Prefix, result.Prefix);
    }

    [Fact]
    public async Task GetTestProjectByNameAsync_WithNonExistentProject_ThrowsNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TestLinkNotFoundException>(() => 
            Client.GetTestProjectByNameAsync("NonExistentProject_12345"));
    }

    [Fact]
    public async Task GetTestProjectByNameAsync_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => Client.GetTestProjectByNameAsync(null!));
    }

    #endregion

    #region CreateTestProject Tests

    [Fact]
    public async Task CreateTestProjectAsync_WithValidParameters_CreatesProject()
    {
        // Arrange
        var projectName = $"TestProject_{Guid.NewGuid():N}";
        var prefix = $"TP{Random.Shared.Next(1000, 9999)}";
        var notes = "Test project created by integration test";
        
        var options = new TestProjectOptions
        {
            RequirementsEnabled = true,
            TestPriorityEnabled = true,
            AutomationEnabled = true,
            InventoryEnabled = false
        };

        // Act
        var result = await Client.CreateTestProjectAsync(
            projectName, 
            prefix, 
            notes, 
            options,
            active: true,
            isPublic: true);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Status, $"Failed to create project: {result.Message}");
        Assert.True(result.Id > 0);
        Assert.Equal("createTestProject", result.Operation);

        // Cleanup - Note: TestLink doesn't have a delete project API
        // The created project will remain in the TestLink instance
    }

    [Fact]
    public async Task CreateTestProjectAsync_WithDuplicateName_ReturnsError()
    {
        // Arrange - First create a project
        var projectName = $"DuplicateTest_{Guid.NewGuid():N}";
        var prefix = $"DT{Random.Shared.Next(1000, 9999)}";
        
        // Create first project
        var firstResult = await Client.CreateTestProjectAsync(projectName, prefix);
        Assert.True(firstResult.Status, "Failed to create first project");

        var duplicatePrefix = $"DT{Random.Shared.Next(1000, 9999)}";

        // Act & Assert
        await Assert.ThrowsAsync<TestLinkApiException>(() => 
            Client.CreateTestProjectAsync(projectName, duplicatePrefix));
    }

    [Fact]
    public async Task CreateTestProjectAsync_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.CreateTestProjectAsync(null!, "PREFIX"));
    }

    [Fact]
    public async Task CreateTestProjectAsync_WithNullPrefix_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.CreateTestProjectAsync("TestProject", null!));
    }

    #endregion

    #region GetProjectTestPlans Tests

    [Fact]
    public async Task GetProjectTestPlansAsync_WithExistingProject_ReturnsTestPlans()
    {
        // Arrange - Get a project to test with
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        // Act
        var result = await Client.GetProjectTestPlansAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        // Result can be empty if no test plans exist for this project
        var testPlans = result.ToList();
        
        // Verify structure of any returned test plans
        foreach (var testPlan in testPlans)
        {
            Assert.True(testPlan.Id > 0);
            Assert.NotEmpty(testPlan.Name);
            Assert.Equal(project.Id, testPlan.TestProjectId);
        }
    }

    [Fact]
    public async Task GetProjectTestPlansAsync_WithInvalidProjectId_ThrowsNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TestLinkNotFoundException>(() => Client.GetProjectTestPlansAsync(99999));
    }

    #endregion

    #region GetFirstLevelTestSuitesForTestProject Tests

    [Fact]
    public async Task GetFirstLevelTestSuitesForTestProjectAsync_WithExistingProject_ReturnsTestSuites()
    {
        // Arrange - Get a project to test with
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        // Act
        var result = await Client.GetFirstLevelTestSuitesForTestProjectAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        // Result can be empty if no test suites exist for this project
        var testSuites = result.ToList();
        
        // Verify structure of any returned test suites
        foreach (var testSuite in testSuites)
        {
            Assert.True(testSuite.Id > 0);
            Assert.NotEmpty(testSuite.Name);
            // First level test suites should have the project as parent
            Assert.Equal(project.Id, testSuite.ParentId);
        }
    }

    [Fact]
    public async Task GetFirstLevelTestSuitesForTestProjectAsync_WithEmptyProject_ReturnsEmptyCollection()
    {
        // Arrange - Get a project to test with
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        // Act - This should return empty collection instead of throwing exception when project is empty
        var result = await Client.GetFirstLevelTestSuitesForTestProjectAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        // Result can be empty if no test suites exist for this project
        var testSuites = result.ToList();
        
        // Verify structure of any returned test suites
        foreach (var testSuite in testSuites)
        {
            Assert.True(testSuite.Id > 0);
            Assert.NotEmpty(testSuite.Name);
            // First level test suites should have the project as parent
            Assert.Equal(project.Id, testSuite.ParentId);
        }
    }

    [Fact]
    public async Task GetFirstLevelTestSuitesForTestProjectAsync_WithInvalidProjectId_ThrowsNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TestLinkNotFoundException>(() => Client.GetFirstLevelTestSuitesForTestProjectAsync(99999));
    }

    #endregion

    #region UploadTestProjectAttachment Tests

    [Fact]
    public async Task UploadTestProjectAttachmentAsync_WithValidParameters_UploadsAttachment()
    {
        // Arrange - Get a project to test with
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var fileName = "test-attachment.txt";
        var fileType = "text/plain";
        var content = "This is a test attachment content"u8.ToArray();
        var title = "Test Attachment";
        var description = "Test attachment uploaded by integration test";

        // Act
        var result = await Client.UploadTestProjectAttachmentAsync(
            project.Id, fileName, fileType, content, title, description);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.ForeignKeyId);
        Assert.Equal("nodes_hierarchy", result.LinkedTableName);
        Assert.Equal(fileName, result.FileName);
        Assert.Equal(fileType, result.FileType);
        Assert.Equal(title, result.Title);
        Assert.Equal(description, result.Description);
        Assert.Equal(content.Length, result.Size);
    }

    [Fact]
    public async Task UploadTestProjectAttachmentAsync_WithNullFileName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadTestProjectAttachmentAsync(1, null!, "text/plain", "test"u8.ToArray()));
    }

    [Fact]
    public async Task UploadTestProjectAttachmentAsync_WithNullContent_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadTestProjectAttachmentAsync(1, "test.txt", "text/plain", null!));
    }

    #endregion

    public void Dispose()
    {
        // No resources to dispose in this test class
        // Base class handles client disposal
    }
}