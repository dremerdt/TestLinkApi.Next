# TestLinkApi.Next

[![NuGet Version](https://img.shields.io/nuget/v/TestLinkApi.Next.svg)](https://www.nuget.org/packages/TestLinkApi.Next/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TestLinkApi.Next.svg)](https://www.nuget.org/packages/TestLinkApi.Next/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A modern, async-first TestLink API client for .NET 8+ with comprehensive XML-RPC support.

## Features

- 🚀 **Async/await support** - All methods are fully asynchronous
- 🎯 **Modern .NET 8+** - Built with the latest .NET features
- 🛡️ **Type-safe models** - Strongly-typed request/response models
- 🔧 **Dependency Injection** - First-class support for Microsoft DI container
- 📦 **Fluent Builder** - Easy configuration with builder pattern
- 🧪 **Comprehensive Testing** - Extensive test coverage
- 🔍 **Rich Error Handling** - Detailed exception information

## Quick Start

### Installation

```bash
dotnet add package TestLinkApi.Next
```

### Basic Usage

```csharp
using TestLinkApi.Next;

// Create client
var client = new TestLinkClient(
    apiKey: "your-developer-key",
    baseUrl: "http://your-testlink/lib/api/xmlrpc/v1/xmlrpc.php"
);

// Test connection
var hello = await client.SayHelloAsync();
Console.WriteLine(hello); // "Hello!"

// Get all projects
var projects = await client.GetProjectsAsync();
foreach (var project in projects)
{
    Console.WriteLine($"Project: {project.Name}");
}
```

### Using with Dependency Injection

```csharp
// In Program.cs or Startup.cs
services.AddTestLinkClient("your-api-key", "http://your-testlink/lib/api/xmlrpc/v1/xmlrpc.php");

// Or with configuration
services.AddTestLinkClient(builder => builder
    .WithApiKey("your-api-key")
    .WithBaseUrl("http://your-testlink/lib/api/xmlrpc/v1/xmlrpc.php")
    .WithTimeout(TimeSpan.FromMinutes(2))
    .WithCertificateValidation(false) // For self-signed certificates
);

// Inject and use
public class TestService
{
    private readonly TestLinkClient _client;

    public TestService(TestLinkClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<TestProject>> GetProjectsAsync()
    {
        return await _client.GetProjectsAsync();
    }
}
```

### Fluent Builder Pattern

```csharp
var client = TestLinkClientBuilder.Create()
    .WithApiKey("your-developer-key")
    .WithBaseUrl("http://your-testlink/lib/api/xmlrpc/v1/xmlrpc.php")
    .WithTimeout(TimeSpan.FromMinutes(5))
    .WithCertificateValidation(false)
    .Build();
```

## Core Operations

### Project Management

```csharp
// Get all projects
var projects = await client.GetProjectsAsync();

// Get project by name
var project = await client.GetTestProjectByNameAsync("MyProject");

// Create new project
var result = await client.CreateTestProjectAsync(
    testProjectName: "New Project",
    testCasePrefix: "NP",
    notes: "Project description",
    options: new TestProjectOptions
    {
        RequirementsEnabled = true,
        TestPriorityEnabled = true,
        AutomationEnabled = true,
        InventoryEnabled = false
    }
);

// Upload project attachment
var content = File.ReadAllBytes("document.pdf");
var attachmentResult = await client.UploadTestProjectAttachmentAsync(
    testProjectId: project.Id,
    fileName: "document.pdf",
    fileType: "application/pdf",
    content: content,
    title: "Project Documentation",
    description: "Important project documentation"
);
```

### Test Plan Management

```csharp
// Create test plan
var planResult = await client.CreateTestPlanAsync(
    testPlanName: "Release 1.0 Test Plan",
    testProjectName: "MyProject",
    notes: "Test plan for release 1.0",
    active: true,
    isPublic: true
);

// Get test plan by name
var testPlan = await client.GetTestPlanByNameAsync("Release 1.0 Test Plan", "MyProject");

// Get test plan totals (statistics)
var totals = await client.GetTotalsForTestPlanAsync(testPlan.Id);

// Get platforms for test plan
var platforms = await client.GetTestPlanPlatformsAsync(testPlan.Id);
```

### Test Suite Management

```csharp
// Create test suite
var suiteResult = await client.CreateTestSuiteAsync(
    testProjectId: project.Id,
    testSuiteName: "Authentication Tests",
    details: "Tests for user authentication functionality",
    parentId: null, // Root level
    order: 1,
    checkDuplicatedName: true,
    actionOnDuplicatedName: "generate_new"
);

// Get test suites for project
var testSuites = await client.GetFirstLevelTestSuitesForTestProjectAsync(project.Id);

// Get test suite by ID
var testSuite = await client.GetTestSuiteByIdAsync(suiteResult.Id);
```

### Test Case Management

```csharp
// Create test case with steps
var steps = new[]
{
    new TestStep(1, "Open login page", "Login page is displayed"),
    new TestStep(2, "Enter valid credentials", "Credentials are accepted"),
    new TestStep(3, "Click login button", "User is logged in successfully")
};

var testCaseRequest = new CreateTestCaseRequest
{
    AuthorLogin = "testuser",
    TestSuiteId = testSuite.Id,
    TestCaseName = "Valid Login Test",
    TestProjectId = project.Id,
    Summary = "Test successful login with valid credentials",
    Preconditions = "User has valid account",
    Steps = steps,
    Keywords = "login,authentication",
    Importance = 2, // Medium
    ExecutionType = 1 // Manual
};

var caseResult = await client.CreateTestCaseAsync(testCaseRequest);

// Get test cases for test suite
var testCases = await client.GetTestCasesForTestSuiteAsync(
    testSuiteId: testSuite.Id,
    deep: true,
    details: "full"
);

// Get test case by ID
var testCase = await client.GetTestCaseAsync(caseResult.Id);

// Upload test case attachment
var screenshotContent = File.ReadAllBytes("screenshot.png");
await client.UploadTestCaseAttachmentAsync(
    testCaseId: caseResult.Id,
    fileName: "screenshot.png",
    fileType: "image/png",
    content: screenshotContent,
    title: "Login Screenshot",
    description: "Screenshot of successful login"
);
```

## Advanced Features

### Error Handling

```csharp
try
{
    var projects = await client.GetProjectsAsync();
}
catch (TestLinkApiException ex)
{
    // TestLink-specific errors
    Console.WriteLine($"TestLink Error: {ex.Message}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
}
catch (HttpRequestException ex)
{
    // Network/HTTP errors
    Console.WriteLine($"Network Error: {ex.Message}");
}
```

### Extension Methods

```csharp
// Execution status helpers
string status = "p";
Console.WriteLine(status.IsSuccess()); // true
Console.WriteLine(status.ToDisplayString()); // "Passed"

// Test step creation
var step = Extensions.CreateStep(
    stepNumber: 1,
    actions: "Perform action",
    expectedResults: "Expected result",
    active: true,
    executionType: TestLinkConstants.ExecutionType.Manual
);

// External ID helpers (for test cases with project prefix)
var fullId = testCase.GetFullExternalId("PROJ"); // "PROJ-123"
```

### Constants

```csharp
// Execution statuses
TestLinkConstants.ExecutionStatus.Pass    // "p"
TestLinkConstants.ExecutionStatus.Fail    // "f"
TestLinkConstants.ExecutionStatus.Blocked // "b"
TestLinkConstants.ExecutionStatus.NotRun  // "n"

// Execution types
TestLinkConstants.ExecutionType.Manual    // 1
TestLinkConstants.ExecutionType.Automated // 2

// Importance levels
TestLinkConstants.Importance.Low    // 1
TestLinkConstants.Importance.Medium // 2
TestLinkConstants.Importance.High   // 3
```

## API Coverage

### ✅ Implemented Operations

- **Basic Operations**: Ping, SayHello, About, CheckDevKey, DoesUserExist, SetTestMode, Repeat, GetFullPath
- **Project Operations**: GetProjects, GetTestProjectByName, CreateTestProject, UploadTestProjectAttachment, GetProjectTestPlans, GetFirstLevelTestSuitesForTestProject
- **Test Plan Operations**: CreateTestPlan, GetTestPlanByName, GetTestPlanPlatforms, GetTotalsForTestPlan, AddTestCaseToTestPlan, GetBuildsForTestPlan, GetLatestBuildForTestPlan, CreateBuild, GetLastExecutionResult, DeleteExecution, ReportTestCaseResult
- **Test Suite Operations**: CreateTestSuite, UploadTestSuiteAttachment, GetTestSuitesForTestPlan, GetTestSuitesForTestSuite, GetTestSuiteById
- **Test Case Operations**: CreateTestCase, UploadTestCaseAttachment, GetTestCasesForTestSuite, GetTestCasesForTestPlan, GetTestCaseIdByName, GetTestCaseCustomFieldDesignValue, GetTestCaseAttachments, GetTestCase
- **Attachment Operations**: UploadAttachment, UploadRequirementSpecificationAttachment, UploadRequirementAttachment  
- **Requirement Operations**: AssignRequirements

### 🚧 Planned Operations

- ...

## Configuration

### HttpClient Configuration

```csharp
var handler = new HttpClientHandler()
{
    // For self-signed certificates
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};

var httpClient = new HttpClient(handler)
{
    Timeout = TimeSpan.FromMinutes(10)
};

var client = new TestLinkClient("api-key", "base-url", httpClient);
```

### Timeout Configuration

```csharp
var client = TestLinkClientBuilder.Create()
    .WithApiKey("your-api-key")
    .WithBaseUrl("your-base-url")
    .WithTimeout(TimeSpan.FromMinutes(2)) // Default is 5 minutes
    .Build();
```

## Testing

The library includes comprehensive integration tests. To run them:

1. Copy `TestLinkApi.Next.Tests/appsettings.example.json` to `appsettings.json`
2. Configure your TestLink instance details
3. Run tests:

```bash
dotnet test
```

## Requirements

- .NET 8.0 or later
- TestLink instance with XML-RPC API enabled
- Valid TestLink developer API key

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Acknowledgments

- Inspired by the original TestLink .NET Core projects
- Built with modern .NET 8 features and best practices
- Special thanks to the TestLink community

---

Created with ❤️ by Mihail Matenco.