using System.Text;
using System.Xml;
using TestLinkApi.Next.Contracts;

namespace TestLinkApi.Next;

/// <summary>
/// Modern TestLink API client with async support for .NET 8+
/// </summary>
public class TestLinkClient : ITestLinkOperations, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private bool _disposed;

    /// <summary>
    /// Creates a new TestLink API client
    /// </summary>
    /// <param name="apiKey">Developer key provided by TestLink</param>
    /// <param name="baseUrl">TestLink API URL (e.g., http://localhost/testlink/lib/api/xmlrpc/v1/xmlrpc.php)</param>
    /// <param name="httpClient">Optional HttpClient instance</param>
    public TestLinkClient(string apiKey, string baseUrl, HttpClient? httpClient = null)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        _httpClient = httpClient ?? new HttpClient();
    }

    #region Basic Operations

    public async Task<string> PingAsync(CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.Ping, new { devKey = _apiKey }, cancellationToken);
        return ParseResponse<string>(response);
    }

    public async Task<string> SayHelloAsync( CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.SayHello, new { devKey = _apiKey }, cancellationToken);
        return ParseResponse<string>(response);
    }

    public async Task<string> AboutAsync(CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.About, new { devKey = _apiKey }, cancellationToken);
        return ParseResponse<string>(response);
    }

    public async Task<string> CheckDevKeyAsync(CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.CheckDevKey, new { devKey = _apiKey }, cancellationToken);
        return ParseResponse<string>(response);
    }

    public async Task<bool> DoesUserExistAsync(string username, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(username);
        var response = await CallApiAsync(TestLinkApiMethods.DoesUserExist, new { devKey = _apiKey, user = username }, cancellationToken);
        return ParseResponse<bool>(response);
    }

    public async Task<bool> SetTestMode(bool set, CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.SetTestMode, new { devKey = _apiKey, testmode = set }, cancellationToken);
        return ParseResponse<bool>(response);
    }

    public async Task<string> RepeatAsync(string text, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        var response = await CallApiAsync(TestLinkApiMethods.Repeat, new { devKey = _apiKey, str = text }, cancellationToken);
        return ParseResponse<string>(response);
    }

    public async Task<string> GetFullPathAsync(int nodeId, CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.GetFullPath, new { devKey = _apiKey, nodeId }, cancellationToken);
        return ParseResponse<string>(response);
    }

    #endregion

    #region Project Operations

    public async Task<IEnumerable<TestProject>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.GetProjects, new { devKey = _apiKey }, cancellationToken);
        return ParseResponse<IEnumerable<TestProject>>(response);
    }

    public async Task<TestProject?> GetTestProjectByNameAsync(string testProjectName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testProjectName);
        
        var response = await CallApiAsync(TestLinkApiMethods.GetProjectByName, 
            new { devKey = _apiKey, testprojectname = testProjectName }, cancellationToken);
        
        // TestLink returns a single project object for this method
        return ParseResponse<TestProject>(response);
    }

    public async Task<GeneralResult> CreateTestProjectAsync(
        string testProjectName,
        string testCasePrefix,
        string? notes = null,
        TestProjectOptions? options = null,
        bool active = true,
        bool isPublic = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testProjectName);
        ArgumentNullException.ThrowIfNull(testCasePrefix);

        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testprojectname", testProjectName },
            { "testcaseprefix", testCasePrefix },
            { "notes", notes ?? string.Empty },
            { "active", active ? 1 : 0 },
            { "public", isPublic ? 1 : 0 }
        };

        // Add options if provided
        if (options != null)
        {
            parameters["options"] = new Dictionary<string, object>
            {
                { "requirementsEnabled", options.RequirementsEnabled ? 1 : 0 },
                { "testPriorityEnabled", options.TestPriorityEnabled ? 1 : 0 },
                { "automationEnabled", options.AutomationEnabled ? 1 : 0 },
                { "inventoryEnabled", options.InventoryEnabled ? 1 : 0 }
            };
        }

        var response = await CallApiAsync(TestLinkApiMethods.CreateProject, parameters, cancellationToken);
        
        // TestLink returns an array with a single GeneralResult struct
        // Parse directly as GeneralResult since the XmlRpcResponseParser handles the array extraction
        return ParseResponse<GeneralResult>(response);
    }

    public async Task<AttachmentRequestResponse> UploadTestProjectAttachmentAsync(
        int testProjectId,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(fileType);
        ArgumentNullException.ThrowIfNull(content);

        var base64Content = Convert.ToBase64String(content);
        
        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testprojectid", testProjectId },
            { "filename", fileName },
            { "filetype", fileType },
            { "content", base64Content },
            { "title", title ?? string.Empty },
            { "description", description ?? string.Empty }
        };

        var response = await CallApiAsync(TestLinkApiMethods.UploadProjectAttachment, parameters, cancellationToken);
        return ParseResponse<AttachmentRequestResponse>(response);
    }

    public async Task<IEnumerable<TestPlan>> GetProjectTestPlansAsync(int testProjectId, CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.GetProjectTestPlans, 
            new { devKey = _apiKey, testprojectid = testProjectId }, cancellationToken);
        
        return ParseResponse<IEnumerable<TestPlan>>(response);
    }

    public async Task<IEnumerable<TestSuite>> GetFirstLevelTestSuitesForTestProjectAsync(int testProjectId, CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.GetFirstLevelTestSuites, 
            new { devKey = _apiKey, testprojectid = testProjectId }, cancellationToken);
        
        return ParseResponse<IEnumerable<TestSuite>>(response);
    }

    #endregion

    #region TestPlan Operations

    public async Task<GeneralResult> CreateTestPlanAsync(
        string testPlanName,
        string testProjectName,
        string? notes = null,
        bool active = true,
        bool isPublic = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testPlanName);
        ArgumentNullException.ThrowIfNull(testProjectName);

        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testplanname", testPlanName },
            { "testprojectname", testProjectName },
            { "notes", notes ?? string.Empty },
            { "active", active ? 1 : 0 },
            { "public", isPublic ? 1 : 0 }
        };

        var response = await CallApiAsync(TestLinkApiMethods.CreateTestPlan, parameters, cancellationToken);
        return ParseResponse<GeneralResult>(response);
    }

    public async Task<TestPlan?> GetTestPlanByNameAsync(
        string testPlanName, 
        string testProjectName, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testPlanName);
        ArgumentNullException.ThrowIfNull(testProjectName);
        
        var response = await CallApiAsync(TestLinkApiMethods.GetTestPlanByName, 
            new { devKey = _apiKey, testplanname = testPlanName, testprojectname = testProjectName }, cancellationToken);
        
        return ParseResponse<TestPlan?>(response);
    }

    public async Task<IEnumerable<TestPlatform>> GetTestPlanPlatformsAsync(
        int testPlanId, 
        CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.GetTestPlanPlatforms, 
            new { devKey = _apiKey, testplanid = testPlanId }, cancellationToken);
        
        return ParseResponse<IEnumerable<TestPlatform>>(response);
    }

    public async Task<IEnumerable<TestPlanTotal>> GetTotalsForTestPlanAsync(
        int testPlanId, 
        CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.GetTotalsForTestPlan, 
            new { devKey = _apiKey, testplanid = testPlanId }, cancellationToken);
        
        return ParseResponse<IEnumerable<TestPlanTotal>>(response);
    }

    public async Task<GeneralResult> AddTestCaseToTestPlanAsync(
        int testProjectId,
        int testPlanId,
        string testCaseExternalId,
        int version,
        int? platformId = null,
        int? executionOrder = null,
        int? urgency = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testCaseExternalId);

        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testprojectid", testProjectId },
            { "testplanid", testPlanId },
            { "testcaseexternalid", testCaseExternalId },
            { "version", version }
        };

        if (platformId.HasValue)
            parameters["platformid"] = platformId.Value;

        if (executionOrder.HasValue)
            parameters["executionorder"] = executionOrder.Value;

        if (urgency.HasValue)
            parameters["urgency"] = urgency.Value;

        var response = await CallApiAsync(TestLinkApiMethods.AddTestCaseToTestPlan, parameters, cancellationToken);
        return ParseResponse<GeneralResult>(response);
    }

    public async Task<IEnumerable<Build>> GetBuildsForTestPlanAsync(
        int testPlanId,
        CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.GetBuildsForTestPlan, 
            new { devKey = _apiKey, testplanid = testPlanId }, cancellationToken);
        
        return ParseResponse<IEnumerable<Build>>(response);
    }

    public async Task<Build?> GetLatestBuildForTestPlanAsync(
        int testPlanId,
        CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.GetLatestBuildForTestPlan, 
            new { devKey = _apiKey, testplanid = testPlanId }, cancellationToken);
        
        return ParseResponse<Build?>(response);
    }

    public async Task<GeneralResult> CreateBuildAsync(
        int testPlanId,
        string buildName,
        string? buildNotes = null,
        bool active = true,
        bool open = true,
        DateTime? releaseDate = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(buildName);

        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testplanid", testPlanId },
            { "buildname", buildName },
            { "buildnotes", buildNotes ?? string.Empty },
            { "active", active ? 1 : 0 },
            { "open", open ? 1 : 0 }
        };

        if (releaseDate.HasValue)
        {
            // TestLink expects date in YYYY-MM-DD format
            parameters["releasedate"] = releaseDate.Value.ToString("yyyy-MM-dd");
        }

        var response = await CallApiAsync(TestLinkApiMethods.CreateBuild, parameters, cancellationToken);
        return ParseResponse<GeneralResult>(response);
    }

    public async Task<ExecutionResult?> GetLastExecutionResultAsync(
        int testPlanId,
        int? testCaseId = null,
        string? testCaseExternalId = null,
        int? platformId = null,
        string? platformName = null,
        int? buildId = null,
        string? buildName = null,
        CancellationToken cancellationToken = default)
    {
        if (testCaseId == null && string.IsNullOrEmpty(testCaseExternalId))
        {
            throw new ArgumentException("Either testCaseId or testCaseExternalId must be provided.");
        }

        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testplanid", testPlanId }
        };

        if (testCaseId.HasValue)
            parameters["testcaseid"] = testCaseId.Value;

        if (!string.IsNullOrEmpty(testCaseExternalId))
            parameters["testcaseexternalid"] = testCaseExternalId;

        if (platformId.HasValue)
            parameters["platformid"] = platformId.Value;

        if (!string.IsNullOrEmpty(platformName))
            parameters["platformname"] = platformName;

        if (buildId.HasValue)
            parameters["buildid"] = buildId.Value;

        if (!string.IsNullOrEmpty(buildName))
            parameters["buildname"] = buildName;

        var response = await CallApiAsync(TestLinkApiMethods.GetLastExecutionResult, parameters, cancellationToken);
        return ParseResponse<ExecutionResult?>(response);
    }

    public async Task<GeneralResult> DeleteExecutionAsync(
        int executionId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "executionid", executionId }
        };

        var response = await CallApiAsync(TestLinkApiMethods.DeleteExecution, parameters, cancellationToken);
        return ParseResponse<GeneralResult>(response);
    }

    #endregion

    #region TestSuite Operations

    public async Task<GeneralResult> CreateTestSuiteAsync(
        int testProjectId,
        string testSuiteName,
        string details,
        int? parentId = null,
        int? order = null,
        bool checkDuplicatedName = true,
        string actionOnDuplicatedName = "generate_new",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testSuiteName);
        ArgumentNullException.ThrowIfNull(details);

        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testprojectid", testProjectId },
            { "testsuitename", testSuiteName },
            { "details", details },
            { "checkduplicatedname", checkDuplicatedName ? 1 : 0 },
            { "actiononduplicatedname", actionOnDuplicatedName }
        };

        if (parentId.HasValue)
        {
            parameters["parentid"] = parentId.Value;
        }

        if (order.HasValue)
        {
            parameters["order"] = order.Value;
        }

        var response = await CallApiAsync(TestLinkApiMethods.CreateTestSuite, parameters, cancellationToken);
        return ParseResponse<GeneralResult>(response);
    }

    public async Task<AttachmentRequestResponse> UploadTestSuiteAttachmentAsync(
        int testSuiteId,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(fileType);
        ArgumentNullException.ThrowIfNull(content);

        var base64Content = Convert.ToBase64String(content);
        
        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testsuiteid", testSuiteId },
            { "filename", fileName },
            { "filetype", fileType },
            { "content", base64Content },
            { "title", title ?? string.Empty },
            { "description", description ?? string.Empty }
        };

        var response = await CallApiAsync(TestLinkApiMethods.UploadTestSuiteAttachment, parameters, cancellationToken);
        return ParseResponse<AttachmentRequestResponse>(response);
    }

    public async Task<IEnumerable<TestSuite>> GetTestSuitesForTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.GetTestSuitesForTestPlan, 
            new { devKey = _apiKey, testplanid = testPlanId }, cancellationToken);
        
        return ParseResponse<IEnumerable<TestSuite>>(response);
    }

    public async Task<IEnumerable<TestSuite>> GetTestSuitesForTestSuiteAsync(int testSuiteId, CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.GetTestSuitesForTestSuite, 
            new { devKey = _apiKey, testsuiteid = testSuiteId }, cancellationToken);
        
        return ParseResponse<IEnumerable<TestSuite>>(response);
    }

    public async Task<TestSuite?> GetTestSuiteByIdAsync(int testSuiteId, CancellationToken cancellationToken = default)
    {
        var response = await CallApiAsync(TestLinkApiMethods.GetTestSuiteById, 
            new { devKey = _apiKey, testsuiteid = testSuiteId }, cancellationToken);
        
        return ParseResponse<TestSuite?>(response);
    }

    #endregion

    #region TestCase Operations

    public async Task<GeneralResult> CreateTestCaseAsync(
        CreateTestCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.AuthorLogin);
        ArgumentNullException.ThrowIfNull(request.TestCaseName);
        ArgumentNullException.ThrowIfNull(request.Summary);

        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "authorlogin", request.AuthorLogin },
            { "testsuiteid", request.TestSuiteId },
            { "testcasename", request.TestCaseName },
            { "testprojectid", request.TestProjectId },
            { "summary", request.Summary },
            { "order", request.Order },
            { "checkduplicatedname", request.CheckDuplicatedName ? 1 : 0 },
            { "actiononduplicatedname", request.ActionOnDuplicatedName },
            { "executiontype", request.ExecutionType },
            { "importance", request.Importance }
        };

        if (!string.IsNullOrEmpty(request.Preconditions))
        {
            parameters["preconditions"] = request.Preconditions;
        }

        if (!string.IsNullOrEmpty(request.Keywords))
        {
            parameters["keywords"] = request.Keywords;
        }

        if (request.Steps != null && request.Steps.Length > 0)
        {
            parameters["steps"] = request.Steps;
        }

        var response = await CallApiAsync(TestLinkApiMethods.CreateTestCase, parameters, cancellationToken);
        return ParseResponse<GeneralResult>(response);
    }

    public async Task<AttachmentRequestResponse> UploadTestCaseAttachmentAsync(
        int testCaseId,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(fileType);
        ArgumentNullException.ThrowIfNull(content);

        var base64Content = Convert.ToBase64String(content);
        
        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testcaseid", testCaseId },
            { "filename", fileName },
            { "filetype", fileType },
            { "content", base64Content },
            { "title", title ?? string.Empty },
            { "description", description ?? string.Empty }
        };

        var response = await CallApiAsync(TestLinkApiMethods.UploadTestCaseAttachment, parameters, cancellationToken);
        return ParseResponse<AttachmentRequestResponse>(response);
    }

    public async Task<IEnumerable<TestCaseFromTestSuite>> GetTestCasesForTestSuiteAsync(
        int testSuiteId,
        bool deep = true,
        string details = "simple",
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testsuiteid", testSuiteId },
            { "deep", deep ? 1 : 0 },
            { "details", details }
        };

        var response = await CallApiAsync(TestLinkApiMethods.GetTestCasesForTestSuite, parameters, cancellationToken);
        return ParseResponse<IEnumerable<TestCaseFromTestSuite>>(response);
    }

    public async Task<IEnumerable<TestCaseFromTestPlan>> GetTestCasesForTestPlanAsync(
        int testPlanId,
        int? testCaseId = null,
        int? buildId = null,
        int? keywordId = null,
        string? keywords = null,
        bool? executed = null,
        int? assignedTo = null,
        string? executeStatus = null,
        int? executionType = null,
        bool getStepsInfo = false,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testplanid", testPlanId },
            { "getstepsinfo", getStepsInfo ? 1 : 0 }
        };

        if (testCaseId.HasValue)
            parameters["testcaseid"] = testCaseId.Value;

        if (buildId.HasValue)
            parameters["buildid"] = buildId.Value;

        if (keywordId.HasValue)
            parameters["keywordid"] = keywordId.Value;

        if (!string.IsNullOrEmpty(keywords))
            parameters["keywords"] = keywords;

        if (executed.HasValue)
            parameters["executed"] = executed.Value ? 1 : 0;

        if (assignedTo.HasValue)
            parameters["assignedto"] = assignedTo.Value;

        if (!string.IsNullOrEmpty(executeStatus))
            parameters["executestatus"] = executeStatus;

        if (executionType.HasValue)
            parameters["executiontype"] = executionType.Value;

        var response = await CallApiAsync(TestLinkApiMethods.GetTestCasesForTestPlan, parameters, cancellationToken);
        return ParseResponse<IEnumerable<TestCaseFromTestPlan>>(response);
    }

    public async Task<IEnumerable<TestCaseId>> GetTestCaseIdByNameAsync(
        string testCaseName,
        string? testSuiteName = null,
        string? testProjectName = null,
        string? testCasePathName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testCaseName);

        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testcasename", testCaseName }
        };

        if (!string.IsNullOrEmpty(testSuiteName))
            parameters["testsuitename"] = testSuiteName;

        if (!string.IsNullOrEmpty(testProjectName))
            parameters["testprojectname"] = testProjectName;

        if (!string.IsNullOrEmpty(testCasePathName))
            parameters["testcasepathname"] = testCasePathName;

        var response = await CallApiAsync(TestLinkApiMethods.GetTestCaseIdByName, parameters, cancellationToken);
        return ParseResponse<IEnumerable<TestCaseId>>(response);
    }

    public async Task<object> GetTestCaseCustomFieldDesignValueAsync(
        string testCaseExternalId,
        int versionNumber,
        int testProjectId,
        string customFieldName,
        string details = "value",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testCaseExternalId);
        ArgumentNullException.ThrowIfNull(customFieldName);

        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testcaseexternalid", testCaseExternalId },
            { "version", versionNumber },
            { "testprojectid", testProjectId },
            { "customfieldname", customFieldName },
            { "details", details }
        };

        var response = await CallApiAsync(TestLinkApiMethods.GetTestCaseCustomFieldValue, parameters, cancellationToken);
        return ParseResponse<object>(response);
    }

    public async Task<IEnumerable<Attachment>> GetTestCaseAttachmentsAsync(
        int testCaseId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testcaseid", testCaseId }
        };

        var response = await CallApiAsync(TestLinkApiMethods.GetTestCaseAttachments, parameters, cancellationToken);
        return ParseResponse<IEnumerable<Attachment>>(response);
    }

    public async Task<TestCase?> GetTestCaseAsync(
        int testCaseId,
        int? versionNumber = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testcaseid", testCaseId }
        };

        if (versionNumber.HasValue)
        {
            parameters["version"] = versionNumber.Value;
        }

        var response = await CallApiAsync(TestLinkApiMethods.GetTestCase, parameters, cancellationToken);
        return ParseResponse<TestCase?>(response);
    }

    #endregion

    #region Execution Operations

    /// <summary>
    /// Report test case execution result
    /// </summary>
    public async Task<GeneralResult> ReportTestCaseResultAsync(
        ReportTestCaseResultRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Status);

        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testcaseid", request.TestCaseId },
            { "testplanid", request.TestPlanId },
            { "status", request.Status },
            { "overwrite", request.Overwrite ? 1 : 0 },
            { "guess", request.Guess ? 1 : 0 }
        };

        if (request.PlatformId.HasValue)
            parameters["platformid"] = request.PlatformId.Value;

        if (!string.IsNullOrEmpty(request.PlatformName))
            parameters["platformname"] = request.PlatformName;

        if (!string.IsNullOrEmpty(request.Notes))
            parameters["notes"] = request.Notes;

        if (request.BuildId.HasValue)
            parameters["buildid"] = request.BuildId.Value;

        if (request.BugId.HasValue)
            parameters["bugid"] = request.BugId.Value;

        var response = await CallApiAsync(TestLinkApiMethods.ReportTestCaseResult, parameters, cancellationToken);
        return ParseResponse<GeneralResult>(response);
    }

    #endregion

    #region Attachment Operations

    /// <summary>
    /// Upload a general attachment
    /// </summary>
    public async Task<AttachmentRequestResponse> UploadAttachmentAsync(
        int foreignKeyId,
        string foreignKeyTable,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(foreignKeyTable);
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(fileType);
        ArgumentNullException.ThrowIfNull(content);

        var base64Content = Convert.ToBase64String(content);
        
        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "fkid", foreignKeyId },
            { "fktable", foreignKeyTable },
            { "filename", fileName },
            { "filetype", fileType },
            { "content", base64Content },
            { "title", title ?? string.Empty },
            { "description", description ?? string.Empty }
        };

        var response = await CallApiAsync(TestLinkApiMethods.UploadAttachment, parameters, cancellationToken);
        return ParseResponse<AttachmentRequestResponse>(response);
    }

    /// <summary>
    /// Upload an attachment to a requirement specification
    /// </summary>
    public async Task<AttachmentRequestResponse> UploadRequirementSpecificationAttachmentAsync(
        int requirementSpecificationId,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(fileType);
        ArgumentNullException.ThrowIfNull(content);

        var base64Content = Convert.ToBase64String(content);
        
        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "reqspecid", requirementSpecificationId },
            { "filename", fileName },
            { "filetype", fileType },
            { "content", base64Content },
            { "title", title ?? string.Empty },
            { "description", description ?? string.Empty }
        };

        var response = await CallApiAsync(TestLinkApiMethods.UploadRequirementSpecificationAttachment, parameters, cancellationToken);
        return ParseResponse<AttachmentRequestResponse>(response);
    }

    /// <summary>
    /// Upload an attachment to a requirement
    /// </summary>
    public async Task<AttachmentRequestResponse> UploadRequirementAttachmentAsync(
        int requirementId,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(fileType);
        ArgumentNullException.ThrowIfNull(content);

        var base64Content = Convert.ToBase64String(content);
        
        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "reqid", requirementId },
            { "filename", fileName },
            { "filetype", fileType },
            { "content", base64Content },
            { "title", title ?? string.Empty },
            { "description", description ?? string.Empty }
        };

        var response = await CallApiAsync(TestLinkApiMethods.UploadRequirementAttachment, parameters, cancellationToken);
        return ParseResponse<AttachmentRequestResponse>(response);
    }

    #endregion

    #region Requirement Operations

    /// <summary>
    /// Assign requirements to a test case
    /// </summary>
    public async Task<GeneralResult> AssignRequirementsAsync(
        AssignRequirementsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.TestCaseExternalId);
        ArgumentNullException.ThrowIfNull(request.RequirementIds);

        var parameters = new Dictionary<string, object>
        {
            { "devKey", _apiKey },
            { "testprojectid", request.TestProjectId },
            { "testcaseexternalid", request.TestCaseExternalId },
            { "requirements", request.RequirementIds }
        };

        var response = await CallApiAsync(TestLinkApiMethods.AssignRequirements, parameters, cancellationToken);
        return ParseResponse<GeneralResult>(response);
    }

    #endregion

    #region Private Methods

    private async Task<string> CallApiAsync(string method, object parameters, CancellationToken cancellationToken)
    {
        var xmlRpcRequest = CreateXmlRpcRequest(method, parameters);
        var content = new StringContent(xmlRpcRequest, Encoding.UTF8, "text/xml");

        var response = await _httpClient.PostAsync(_baseUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static string CreateXmlRpcRequest(string method, object parameters)
    {
        var doc = new XmlDocument();
        var methodCall = doc.CreateElement("methodCall");
        doc.AppendChild(methodCall);

        var methodName = doc.CreateElement("methodName");
        methodName.InnerText = method;
        methodCall.AppendChild(methodName);

        var paramsElement = doc.CreateElement("params");
        methodCall.AppendChild(paramsElement);

        var param = doc.CreateElement("param");
        paramsElement.AppendChild(param);

        var value = doc.CreateElement("value");
        param.AppendChild(value);

        var structElement = CreateStructFromObject(doc, parameters);
        value.AppendChild(structElement);

        return doc.OuterXml;
    }

    private static XmlElement CreateStructFromObject(XmlDocument doc, object obj)
    {
        var structElement = doc.CreateElement("struct");

        if (obj is Dictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                var member = doc.CreateElement("member");
                structElement.AppendChild(member);

                var name = doc.CreateElement("name");
                name.InnerText = kvp.Key;
                member.AppendChild(name);

                var value = doc.CreateElement("value");
                member.AppendChild(value);

                AddValueToElement(doc, value, kvp.Value);
            }
        }
        else
        {
            var properties = obj.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var member = doc.CreateElement("member");
                structElement.AppendChild(member);

                var name = doc.CreateElement("name");
                name.InnerText = prop.Name;
                member.AppendChild(name);

                var value = doc.CreateElement("value");
                member.AppendChild(value);

                var propValue = prop.GetValue(obj);
                AddValueToElement(doc, value, propValue);
            }
        }

        return structElement;
    }

    private static void AddValueToElement(XmlDocument doc, XmlElement valueElement, object? obj)
    {
        switch (obj)
        {
            case null:
                valueElement.InnerText = "";
                break;
            case string str:
                var stringElement = doc.CreateElement("string");
                stringElement.InnerText = str;
                valueElement.AppendChild(stringElement);
                break;
            case int i:
                var intElement = doc.CreateElement("int");
                intElement.InnerText = i.ToString();
                valueElement.AppendChild(intElement);
                break;
            case bool b:
                var boolElement = doc.CreateElement("boolean");
                boolElement.InnerText = b ? "1" : "0";
                valueElement.AppendChild(boolElement);
                break;
            case Array array:
                var arrayElement = doc.CreateElement("array");
                var dataElement = doc.CreateElement("data");
                arrayElement.AppendChild(dataElement);

                foreach (var item in array)
                {
                    var itemValue = doc.CreateElement("value");
                    dataElement.AppendChild(itemValue);
                    AddValueToElement(doc, itemValue, item);
                }
                valueElement.AppendChild(arrayElement);
                break;
            case Dictionary<string, object> dictObj:
                var dictStructElement = CreateStructFromObject(doc, dictObj);
                valueElement.AppendChild(dictStructElement);
                break;
            default:
                if (obj != null)
                {
                    var objStructElement = CreateStructFromObject(doc, obj);
                    valueElement.AppendChild(objStructElement);
                }
                break;
        }
    }

    private static T ParseResponse<T>(string xmlResponse)
    {
        return XmlRpcResponseParser.ParseResponse<T>(xmlResponse);
    }

    #endregion

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}