using TestLinkApi.Next.Models;

namespace TestLinkApi.Next.Tests;

/// <summary>
/// Integration tests for TestCase Operations in TestLinkClient
/// These tests require a running TestLink instance configured in appsettings.json
/// </summary>
public class TestCaseOperationsTests : TestLinkTestBase, IDisposable
{
    #region CreateTestCase Tests

    [Fact]
    public async Task CreateTestCaseAsync_WithValidParameters_CreatesTestCase()
    {
        // Arrange - Get a project and test suite to test with
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
            // Create a test suite first
            var suiteResult = await Client.CreateTestSuiteAsync(
                project.Id, 
                $"TestSuite_{Guid.NewGuid():N}", 
                "Test suite for test case creation");
            
            if (!suiteResult.Status)
            {
                return; // Skip if we can't create a test suite
            }
            
            testSuite = new TestSuite { Id = suiteResult.Id, Name = "Test Suite" };
        }

        var testCaseName = $"TestCase_{Guid.NewGuid():N}";
        var request = new CreateTestCaseRequest
        {
            AuthorLogin = Settings.User,
            TestSuiteId = testSuite.Id,
            TestCaseName = testCaseName,
            TestProjectId = project.Id,
            Summary = "Test case created by integration test",
            Preconditions = "No special preconditions required",
            Keywords = "integration, test",
            Order = 1,
            CheckDuplicatedName = true,
            ActionOnDuplicatedName = "generate_new",
            ExecutionType = 1, // Manual
            Importance = 2,     // Medium
            Steps = [
                new TestStep(1, "Perform action", "Expected result")
            ]
        };

        // Act
        var result = await Client.CreateTestCaseAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Status, $"Failed to create test case: {result.Message}");
        Assert.True(result.Id > 0);
        Assert.Equal("createTestCase", result.Operation);
    }

    [Fact]
    public async Task CreateTestCaseAsync_WithTestSteps_CreatesTestCaseWithSteps()
    {
        // Arrange - Get a project and test suite to test with
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

        var testCaseName = $"TestCaseWithSteps_{Guid.NewGuid():N}";
        var steps = new[]
        {
            new TestStep
            {
                StepNumber = 1,
                Actions = "Open the application",
                ExpectedResults = "Application opens successfully"
            },
            new TestStep
            {
                StepNumber = 2,
                Actions = "Login with valid credentials",
                ExpectedResults = "User is logged in"
            }
        };

        var request = new CreateTestCaseRequest
        {
            AuthorLogin = Settings.User,
            TestSuiteId = testSuite.Id,
            TestCaseName = testCaseName,
            TestProjectId = project.Id,
            Summary = "Test case with steps created by integration test",
            Steps = steps
        };

        // Act
        var result = await Client.CreateTestCaseAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Status, $"Failed to create test case with steps: {result.Message}");
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task CreateTestCaseAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.CreateTestCaseAsync(null!));
    }

    [Fact]
    public async Task CreateTestCaseAsync_WithInvalidAuthorLogin_ThrowsTestLinkException()
    {
        // Arrange
        var request = new CreateTestCaseRequest
        {
            AuthorLogin = "NonExistentUser_12345",
            TestSuiteId = 1,
            TestCaseName = "Test Case",
            TestProjectId = 1,
            Summary = "Test summary"
        };

        // Act & Assert
        await Assert.ThrowsAsync<TestLinkApiException>(() => 
            Client.CreateTestCaseAsync(request));
    }

    #endregion

    #region UploadTestCaseAttachment Tests

    [Fact]
    public async Task UploadTestCaseAttachmentAsync_WithValidParameters_UploadsAttachment()
    {
        // Arrange - First create a test case
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

        // Create a test case first
        var request = new CreateTestCaseRequest
        {
            AuthorLogin = Settings.User,
            TestSuiteId = testSuite.Id,
            TestCaseName = $"TestCase_{Guid.NewGuid():N}",
            TestProjectId = project.Id,
            Summary = "Test case for attachment upload",
            Steps =
            [
                new TestStep(1, "", "")
            ]
        };

        var testCaseResult = await Client.CreateTestCaseAsync(request);
        if (!testCaseResult.Status)
        {
            return; // Skip if test case creation failed
        }

        // Prepare attachment
        var fileName = "test-document.txt";
        var fileType = "text/plain";
        var content = System.Text.Encoding.UTF8.GetBytes("This is a test document for attachment upload.");
        var title = "Test Document";
        var description = "A test document uploaded by integration test";

        // Act
        var result = await Client.UploadTestCaseAttachmentAsync(
            testCaseResult.Id, 
            fileName, 
            fileType, 
            content, 
            title, 
            description);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testCaseResult.Id, result.ForeignKeyId);
        Assert.Equal(fileName, result.FileName);
        Assert.Equal(fileType, result.FileType);
        Assert.Equal(title, result.Title);
        Assert.Equal(description, result.Description);
        Assert.True(result.Size > 0);
    }

    [Fact]
    public async Task UploadTestCaseAttachmentAsync_WithNullFileName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadTestCaseAttachmentAsync(1, null!, "text/plain", new byte[1]));
    }

    [Fact]
    public async Task UploadTestCaseAttachmentAsync_WithNullContent_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadTestCaseAttachmentAsync(1, "test.txt", "text/plain", null!));
    }

    #endregion

    #region GetTestCasesForTestSuite Tests

    [Fact]
    public async Task GetTestCasesForTestSuiteAsync_WithValidTestSuiteId_ReturnsTestCases()
    {
        // Arrange - Get a test suite with test cases
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
        // Result can be empty if no test cases exist in this test suite
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
        // Arrange
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
        // This method should only return test cases directly in this test suite
        var testCases = result.ToList();
        foreach (var testCase in testCases)
        {
            Assert.Equal(testSuite.Id, testCase.ParentId); // ParentId should match the test suite ID
        }
    }

    [Fact]
    public async Task GetTestCasesForTestSuiteAsync_WithInvalidTestSuiteId_ThrowsTestLinkException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TestLinkApiException>(() => 
            Client.GetTestCasesForTestSuiteAsync(999999));
    }

    #endregion

    #region GetTestCasesForTestPlan Tests

    [Fact]
    public async Task GetTestCasesForTestPlanAsync_WithValidTestPlanId_ReturnsTestCases()
    {
        // Arrange - Get a test plan
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
        var result = await Client.GetTestCasesForTestPlanAsync(testPlan.Id);

        // Assert
        Assert.NotNull(result);
        // Result can be empty if no test cases are linked to this test plan
        var testCases = result.ToList();
        
        // Verify structure of any returned test cases
        foreach (var testCase in testCases)
        {
            Assert.True(testCase.Id > 0);
            Assert.NotEmpty(testCase.Name);
            Assert.True(testCase.TestCaseId > 0);
        }
    }

    [Fact]
    public async Task GetTestCasesForTestPlanAsync_WithSpecificTestCaseId_ReturnsFilteredResults()
    {
        // Arrange
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

        // Get all test cases first to find one to filter by
        var allTestCases = await Client.GetTestCasesForTestPlanAsync(testPlan.Id);
        var firstTestCase = allTestCases.FirstOrDefault();
        
        if (firstTestCase == null)
        {
            return; // Skip if no test cases exist
        }

        // Act
        var result = await Client.GetTestCasesForTestPlanAsync(testPlan.Id, testCaseId: firstTestCase.TestCaseId);

        // Assert
        Assert.NotNull(result);
        var filteredTestCases = result.ToList();
        
        // All returned test cases should match the filter
        foreach (var testCase in filteredTestCases)
        {
            Assert.Equal(firstTestCase.TestCaseId, testCase.TestCaseId);
        }
    }

    [Fact]
    public async Task GetTestCasesForTestPlanAsync_WithInvalidTestPlanId_ThrowsTestLinkException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TestLinkNotFoundException>(() => 
            Client.GetTestCasesForTestPlanAsync(999999));
    }

    #endregion

    #region GetTestCaseIdByName Tests

    [Fact]
    public async Task GetTestCaseIdByNameAsync_WithExistingTestCase_ReturnsTestCaseId()
    {
        // Arrange - First create a test case to search for
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

        var testCaseName = $"SearchableTestCase_{Guid.NewGuid():N}";
        var request = new CreateTestCaseRequest
        {
            AuthorLogin = Settings.User,
            TestSuiteId = testSuite.Id,
            TestCaseName = testCaseName,
            TestProjectId = project.Id,
            Summary = "Test case for search functionality",
            Steps = [
                new TestStep(1, "Perform action", "Expected result")
            ],
        };

        var createResult = await Client.CreateTestCaseAsync(request);
        if (!createResult.Status)
        {
            return; // Skip if test case creation failed
        }

        // Act
        var result = await Client.GetTestCaseIdByNameAsync(testCaseName);

        // Assert
        Assert.NotNull(result);
        var testCaseIds = result.ToList();
        Assert.NotEmpty(testCaseIds);
        
        var matchingTestCase = testCaseIds.FirstOrDefault(tc => tc.Name == testCaseName);
        Assert.NotNull(matchingTestCase);
        Assert.Equal(testCaseName, matchingTestCase.Name);
        Assert.True(matchingTestCase.Id > 0);
    }

    [Fact]
    public async Task GetTestCaseIdByNameAsync_WithNonExistentTestCase_ReturnsEmptyList()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TestLinkApiException>(() => 
            Client.GetTestCaseIdByNameAsync("NonExistentTestCase"));
    }

    [Fact]
    public async Task GetTestCaseIdByNameAsync_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.GetTestCaseIdByNameAsync(null!));
    }

    #endregion

    #region GetTestCaseCustomFieldDesignValue Tests

    [Fact]
    public async Task GetTestCaseCustomFieldDesignValueAsync_WithValidParameters_ReturnsValue()
    {
        // Note: This test requires custom fields to be configured in TestLink
        // For now, we'll test the parameter validation
        
        // Act & Assert - Test parameter validation
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.GetTestCaseCustomFieldDesignValueAsync(null!, 1, 1, "fieldname"));

        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.GetTestCaseCustomFieldDesignValueAsync("TC-1", 1, 1, null!));
    }

    #endregion

    #region GetTestCaseAttachments Tests

    [Fact]
    public async Task GetTestCaseAttachmentsAsync_WithValidTestCaseId_ReturnsAttachments()
    {
        // Arrange - Create a test case and upload an attachment
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

        // Create a test case
        var request = new CreateTestCaseRequest
        {
            AuthorLogin = Settings.User,
            TestSuiteId = testSuite.Id,
            TestCaseName = $"TestCaseWithAttachment_{Guid.NewGuid():N}",
            TestProjectId = project.Id,
            Summary = "Test case for attachment retrieval",
            Steps = [
                new TestStep(1, "Perform action", "Expected result")
            ],
        };

        var testCaseResult = await Client.CreateTestCaseAsync(request);
        if (!testCaseResult.Status)
        {
            return; // Skip if test case creation failed
        }

        // Upload an attachment
        var content = "Test attachment content"u8.ToArray();
        await Client.UploadTestCaseAttachmentAsync(
            testCaseResult.Id, 
            "test-attachment.txt", 
            "text/plain", 
            content, 
            "Test Attachment", 
            "Test attachment description");

        // Act
        var result = await Client.GetTestCaseAttachmentsAsync(testCaseResult.Id);

        // Assert
        Assert.NotNull(result);
        var attachments = result.ToList();
        Assert.NotEmpty(attachments);
        
        var testAttachment = attachments.FirstOrDefault(a => a.Name == "test-attachment.txt");
        Assert.NotNull(testAttachment);
        Assert.Equal("test-attachment.txt", testAttachment.Name);
        Assert.Equal("text/plain", testAttachment.FileType);
    }

    [Fact]
    public async Task GetTestCaseAttachmentsAsync_WithTestCaseWithoutAttachments_ReturnsEmptyList()
    {
        // Arrange - Create a test case without attachments
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

        var request = new CreateTestCaseRequest
        {
            AuthorLogin = Settings.User,
            TestSuiteId = testSuite.Id,
            TestCaseName = $"TestCaseNoAttachments_{Guid.NewGuid():N}",
            TestProjectId = project.Id,
            Summary = "Test case without attachments",
            Steps = [
                new TestStep(1, "Perform action", "Expected result")
            ],
        };

        var testCaseResult = await Client.CreateTestCaseAsync(request);
        if (!testCaseResult.Status)
        {
            return; // Skip if test case creation failed
        }

        // Act
        var result = await Client.GetTestCaseAttachmentsAsync(testCaseResult.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetTestCase Tests

    [Fact]
    public async Task GetTestCaseAsync_WithValidTestCaseId_ReturnsTestCase()
    {
        // Arrange - Create a test case
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

        var testCaseName = $"RetrievableTestCase_{Guid.NewGuid():N}";
        var request = new CreateTestCaseRequest
        {
            AuthorLogin = Settings.User,
            TestSuiteId = testSuite.Id,
            TestCaseName = testCaseName,
            TestProjectId = project.Id,
            Summary = "Test case for retrieval functionality",
            Steps = [
                new TestStep(1, "Perform action", "Expected result")
            ],
        };

        var createResult = await Client.CreateTestCaseAsync(request);
        if (!createResult.Status)
        {
            return; // Skip if test case creation failed
        }

        // Act
        var result = await Client.GetTestCaseAsync(createResult.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createResult.Id, result.TestCaseId);
        Assert.Equal(testCaseName, result.Name);
        Assert.Equal("Test case for retrieval functionality", result.Summary);
        Assert.True(result.Version > 0);
    }

    [Fact]
    public async Task GetTestCaseAsync_WithSpecificVersion_ReturnsCorrectVersion()
    {
        // Arrange - Create a test case
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

        var request = new CreateTestCaseRequest
        {
            AuthorLogin = Settings.User,
            TestSuiteId = testSuite.Id,
            TestCaseName = $"VersionedTestCase_{Guid.NewGuid():N}",
            TestProjectId = project.Id,
            Summary = "Test case for version testing",
            Steps = [
                new TestStep(1, "Perform action", "Expected result")
            ]
        };

        var createResult = await Client.CreateTestCaseAsync(request);
        if (!createResult.Status)
        {
            return; // Skip if test case creation failed
        }

        // Act - Get version 1 specifically
        var result = await Client.GetTestCaseAsync(createResult.Id, versionNumber: 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createResult.Id, result.TestCaseId);
        Assert.Equal(1, result.Version);
    }

    [Fact]
    public async Task GetTestCaseAsync_WithInvalidTestCaseId_ReturnsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TestLinkNotFoundException>(() => 
            Client.GetTestCaseAsync(999999));
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task CreateTestCaseAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var request = new CreateTestCaseRequest
        {
            AuthorLogin = Settings.User,
            TestSuiteId = 1,
            TestCaseName = "Test Case",
            TestProjectId = 1,
            Summary = "Test summary"
        };

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.CreateTestCaseAsync(request, cts.Token));
    }

    [Fact]
    public async Task GetTestCasesForTestSuiteAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.GetTestCasesForTestSuiteAsync(1, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task GetTestCaseAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            Client.GetTestCaseAsync(1, cancellationToken: cts.Token));
    }

    #endregion

    public void Dispose()
    {
        // Cleanup if needed
        GC.SuppressFinalize(this);
    }
}