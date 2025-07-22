using TestLinkApi.Next.Models;

namespace TestLinkApi.Next.Tests;

/// <summary>
/// Integration tests for TestSuite Operations in TestLinkClient
/// These tests require a running TestLink instance configured in appsettings.json
/// </summary>
public class TestSuiteOperationsTests : TestLinkTestBase, IDisposable
{
    #region CreateTestSuite Tests

    [Fact]
    public async Task CreateTestSuiteAsync_WithValidParameters_CreatesTestSuite()
    {
        // Arrange - Get a project to test with
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var suiteName = $"TestSuite_{Guid.NewGuid():N}";
        var details = "Test suite created by integration test";

        // Act
        var result = await Client.CreateTestSuiteAsync(
            project.Id, 
            suiteName, 
            details);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Status, $"Failed to create test suite: {result.Message}");
        Assert.True(result.Id > 0);
        Assert.Equal("createTestSuite", result.Operation);
    }

    [Fact]
    public async Task CreateTestSuiteAsync_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.CreateTestSuiteAsync(1, null!, "details"));
    }

    [Fact]
    public async Task CreateTestSuiteAsync_WithNullDetails_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.CreateTestSuiteAsync(1, "TestSuite", null!));
    }

    #endregion

    #region GetTestSuitesForTestPlan Tests

    [Fact]
    public async Task GetTestSuitesForTestPlanAsync_WithValidTestPlanId_ReturnsTestSuites()
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
        var result = await Client.GetTestSuitesForTestPlanAsync(testPlan.Id);

        // Assert
        Assert.NotNull(result);
        // Result can be empty if no test suites are assigned to this test plan
        var testSuites = result.ToList();
        
        // Verify structure of any returned test suites
        foreach (var testSuite in testSuites)
        {
            Assert.True(testSuite.Id > 0);
            Assert.NotEmpty(testSuite.Name);
        }
    }

    #endregion

    #region GetTestSuitesForTestSuite Tests

    [Fact]
    public async Task GetTestSuitesForTestSuiteAsync_WithValidTestSuiteId_ReturnsChildTestSuites()
    {
        // Arrange - Get a project and its test suites
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testSuites = await Client.GetFirstLevelTestSuitesForTestProjectAsync(project.Id);
        var testSuite = testSuites.FirstOrDefault();
        
        if (testSuite == null)
        {
            return; // Skip if no test suites exist
        }

        // Act
        var result = await Client.GetTestSuitesForTestSuiteAsync(testSuite.Id);

        // Assert
        Assert.NotNull(result);
        // Result can be empty if this test suite has no child test suites
        var childSuites = result.ToList();
        
        // Verify structure of any returned child test suites
        foreach (var childSuite in childSuites)
        {
            Assert.True(childSuite.Id > 0);
            Assert.NotEmpty(childSuite.Name);
            // Child suites should have this test suite as parent
            Assert.Equal(testSuite.Id, childSuite.ParentId);
        }
    }

    #endregion

    #region GetTestCasesForTestSuite Tests

    [Fact]
    public async Task GetTestCasesForTestSuiteAsync_WithValidTestSuiteId_ReturnsTestCases()
    {
        // Arrange - Get a project and its test suites
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testSuites = await Client.GetFirstLevelTestSuitesForTestProjectAsync(project.Id);
        var testSuite = testSuites.FirstOrDefault();
        
        if (testSuite == null)
        {
            return; // Skip if no test suites exist
        }

        // Act
        var result = await Client.GetTestCasesForTestSuiteAsync(testSuite.Id);

        // Assert
        Assert.NotNull(result);
        // Result can be empty if this test suite has no test cases
        var testCases = result.ToList();
        
        // Verify structure of any returned test cases
        foreach (var testCase in testCases)
        {
            Assert.True(testCase.Id > 0);
            Assert.NotEmpty(testCase.Name);
            Assert.Equal(testSuite.Id, testCase.ParentId); // ParentId should match the test suite ID
        }
    }

    [Fact]
    public async Task GetTestCasesForTestSuiteAsync_WithDeepFalse_ReturnsOnlyDirectTestCases()
    {
        // Arrange - Get a project and its test suites
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testSuites = await Client.GetFirstLevelTestSuitesForTestProjectAsync(project.Id);
        var testSuite = testSuites.FirstOrDefault();
        
        if (testSuite == null)
        {
            return; // Skip if no test suites exist
        }

        // Act
        var result = await Client.GetTestCasesForTestSuiteAsync(testSuite.Id, deep: false);

        // Assert
        Assert.NotNull(result);
        // Result can be empty if this test suite has no direct test cases
        var testCases = result.ToList();
        
        // Verify structure of any returned test cases
        foreach (var testCase in testCases)
        {
            Assert.True(testCase.Id > 0);
            Assert.NotEmpty(testCase.Name);
            Assert.Equal(testSuite.Id, testCase.ParentId); // ParentId should match the test suite ID
        }
    }

    #endregion

    #region GetTestSuiteById Tests

    [Fact]
    public async Task GetTestSuiteByIdAsync_WithValidId_ReturnsTestSuite()
    {
        // Arrange - Get a project and its test suites
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testSuites = await Client.GetFirstLevelTestSuitesForTestProjectAsync(project.Id);
        var testSuite = testSuites.FirstOrDefault();
        
        if (testSuite == null)
        {
            return; // Skip if no test suites exist
        }

        // Act
        var result = await Client.GetTestSuiteByIdAsync(testSuite.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testSuite.Id, result.Id);
        Assert.Equal(testSuite.Name, result.Name);
    }

    [Fact]
    public async Task GetTestSuiteByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TestLinkApiException>(() => 
            Client.GetTestSuiteByIdAsync(99999));
    }

    #endregion

    #region UploadTestSuiteAttachment Tests

    [Fact]
    public async Task UploadTestSuiteAttachmentAsync_WithValidParameters_UploadsAttachment()
    {
        // Arrange - Get a project and its test suites
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var testSuites = await Client.GetFirstLevelTestSuitesForTestProjectAsync(project.Id);
        var testSuite = testSuites.FirstOrDefault();
        
        if (testSuite == null)
        {
            return; // Skip if no test suites exist
        }

        var fileName = "test-suite-attachment.txt";
        var fileType = "text/plain";
        var content = "This is a test suite attachment content"u8.ToArray();
        var title = "Test Suite Attachment";
        var description = "Test suite attachment uploaded by integration test";

        // Act
        var result = await Client.UploadTestSuiteAttachmentAsync(
            testSuite.Id, fileName, fileType, content, title, description);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testSuite.Id, result.ForeignKeyId);
        Assert.Equal("nodes_hierarchy", result.LinkedTableName);
        Assert.Equal(fileName, result.FileName);
        Assert.Equal(fileType, result.FileType);
        Assert.Equal(title, result.Title);
        Assert.Equal(description, result.Description);
        Assert.Equal(content.Length, result.Size);
    }

    [Fact]
    public async Task UploadTestSuiteAttachmentAsync_WithNullFileName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadTestSuiteAttachmentAsync(1, null!, "text/plain", "test"u8.ToArray()));
    }

    [Fact]
    public async Task UploadTestSuiteAttachmentAsync_WithNullContent_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadTestSuiteAttachmentAsync(1, "test.txt", "text/plain", null!));
    }

    #endregion

    public void Dispose()
    {
        // No resources to dispose in this test class
        // Base class handles client disposal
    }
}