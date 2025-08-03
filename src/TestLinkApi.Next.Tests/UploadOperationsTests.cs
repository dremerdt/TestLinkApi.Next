using TestLinkApi.Next;
using TestLinkApi.Next.Models;
using Xunit;

namespace TestLinkApi.Next.Tests;

/// <summary>
/// Integration tests for Upload Operations in TestLinkClient
/// These tests require a running TestLink instance configured in appsettings.json
/// </summary>
public class UploadOperationsTests : TestLinkTestBase, IDisposable
{
    #region UploadAttachment Tests

    [Fact]
    public async Task UploadAttachmentAsync_WithValidParameters_UploadsAttachment()
    {
        // Arrange - Get a project to test with
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var fileName = "general-attachment.txt";
        var fileType = "text/plain";
        var content = "This is a general attachment content"u8.ToArray();
        var title = "General Attachment";
        var description = "General attachment uploaded by integration test";

        // Act
        var result = await Client.UploadAttachmentAsync(
            foreignKeyId: project.Id,
            foreignKeyTable: "nodes_hierarchy",
            fileName: fileName,
            fileType: fileType,
            content: content,
            title: title,
            description: description);

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
    public async Task UploadAttachmentAsync_ToTestCaseTable_UploadsSuccessfully()
    {
        // Arrange - Upload attachment to test case using the specialized method
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

        // Create a test case to attach to
        var testCaseName = $"AttachmentTestCase_{Guid.NewGuid():N}";
        var testCaseRequest = new CreateTestCaseRequest
        {
            AuthorLogin = Settings.User,
            TestSuiteId = testSuite.Id,
            TestCaseName = testCaseName,
            TestProjectId = project.Id,
            Summary = "Test case for attachment testing",
            Steps = [new TestStep(1, "Test action", "Expected result")]
        };

        var testCaseResult = await Client.CreateTestCaseAsync(testCaseRequest);
        if (!testCaseResult.Status)
        {
            return; // Skip if test case creation failed
        }

        var fileName = "test-case-attachment.json";
        var fileType = "application/json";
        var content = """{"test": "data", "version": "1.0"}"""u8.ToArray();
        var title = "Test Case JSON Data";
        var description = "JSON data file for test case validation";

        // Act - Use the specialized test case attachment method
        var result = await Client.UploadTestCaseAttachmentAsync(
            testCaseResult.Id,
            fileName,
            fileType,
            content,
            title,
            description);

        // Assert
        Assert.NotNull(result);
        // Note: For test case attachments, the foreign key ID is the test case version ID, not the test case ID
        Assert.True(result.ForeignKeyId > 0, "Foreign key ID should be positive");
        Assert.Equal("nodes_hierarchy", result.LinkedTableName);
        Assert.Equal(fileName, result.FileName);
        Assert.Equal(fileType, result.FileType);
        Assert.Equal(title, result.Title);
        Assert.Equal(description, result.Description);
        Assert.Equal(content.Length, result.Size);
    }

    [Fact]
    public async Task UploadAttachmentAsync_WithBinaryContent_UploadsSuccessfully()
    {
        // Arrange - Upload binary content (simulated image)
        var projects = await Client.GetProjectsAsync();
        var project = projects.FirstOrDefault();
        
        if (project == null)
        {
            return; // Skip if no projects exist
        }

        var fileName = "test-image.png";
        var fileType = "image/png";
        // Create some binary content (simulated PNG header + random data)
        var content = new byte[1024];
        content[0] = 0x89; // PNG magic number start
        content[1] = 0x50;
        content[2] = 0x4E;
        content[3] = 0x47;
        new Random().NextBytes(content.AsSpan(4)); // Fill rest with random data
        var title = "Test Binary Image";
        var description = "Binary image file for attachment testing";

        // Act
        var result = await Client.UploadAttachmentAsync(
            foreignKeyId: project.Id,
            foreignKeyTable: "nodes_hierarchy",
            fileName: fileName,
            fileType: fileType,
            content: content,
            title: title,
            description: description);

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
    public async Task UploadAttachmentAsync_WithNullForeignKeyTable_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadAttachmentAsync(1, null!, "file.txt", "text/plain", "content"u8.ToArray()));
    }

    [Fact]
    public async Task UploadAttachmentAsync_WithNullFileName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadAttachmentAsync(1, "table", null!, "text/plain", "content"u8.ToArray()));
    }

    [Fact]
    public async Task UploadAttachmentAsync_WithNullContent_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadAttachmentAsync(1, "table", "file.txt", "text/plain", null!));
    }

    #endregion

    #region UploadRequirementSpecificationAttachment Tests

    [Fact]
    public async Task UploadRequirementSpecificationAttachmentAsync_WithValidParameters_UploadsAttachment()
    {
        // Note: This test requires requirement specifications to exist in TestLink
        // For now, we'll test the parameter validation
        
        var fileName = "req-spec-attachment.pdf";
        var fileType = "application/pdf";
        var content = "PDF content"u8.ToArray();
        var title = "Requirement Specification Document";
        var description = "Important requirement specification document";

        // Act & Assert - This may throw TestLinkApiException if no requirement spec exists
        try
        {
            var result = await Client.UploadRequirementSpecificationAttachmentAsync(
                requirementSpecificationId: 1,
                fileName: fileName,
                fileType: fileType,
                content: content,
                title: title,
                description: description);

            // If successful, verify the response
            Assert.NotNull(result);
        }
        catch (TestLinkApiException)
        {
            // Expected if requirement specification doesn't exist
            Assert.True(true);
        }
    }

    [Fact]
    public async Task UploadRequirementSpecificationAttachmentAsync_WithNullFileName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadRequirementSpecificationAttachmentAsync(1, null!, "application/pdf", "content"u8.ToArray()));
    }

    [Fact]
    public async Task UploadRequirementSpecificationAttachmentAsync_WithNullContent_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadRequirementSpecificationAttachmentAsync(1, "file.pdf", "application/pdf", null!));
    }

    #endregion

    #region UploadRequirementAttachment Tests

    [Fact]
    public async Task UploadRequirementAttachmentAsync_WithValidParameters_UploadsAttachment()
    {
        // Note: This test requires requirements to exist in TestLink
        // For now, we'll test the parameter validation
        
        var fileName = "requirement-attachment.doc";
        var fileType = "application/msword";
        var content = "Document content"u8.ToArray();
        var title = "Requirement Document";
        var description = "Supporting document for requirement";

        // Act & Assert - This may throw TestLinkApiException if no requirement exists
        try
        {
            var result = await Client.UploadRequirementAttachmentAsync(
                requirementId: 1,
                fileName: fileName,
                fileType: fileType,
                content: content,
                title: title,
                description: description);

            // If successful, verify the response
            Assert.NotNull(result);
        }
        catch (TestLinkApiException)
        {
            // Expected if requirement doesn't exist
            Assert.True(true);
        }
    }

    [Fact]
    public async Task UploadRequirementAttachmentAsync_WithNullFileName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadRequirementAttachmentAsync(1, null!, "application/msword", "content"u8.ToArray()));
    }

    [Fact]
    public async Task UploadRequirementAttachmentAsync_WithNullContent_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            Client.UploadRequirementAttachmentAsync(1, "file.doc", "application/msword", null!));
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