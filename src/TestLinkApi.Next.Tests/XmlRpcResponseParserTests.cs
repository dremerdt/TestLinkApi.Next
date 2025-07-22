using TestLinkApi.Next;
using TestLinkApi.Next.Models;
using Xunit;

namespace TestLinkApi.Next.Tests;

public class XmlRpcResponseParserTests
{
    [Fact]
    public void ParseResponse_ShouldThrowTestLinkValidationException_WhenParameterRequiredError()
    {
        // Arrange - The XML error response provided by the user
        var errorXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>code</name><value><int>200</int></value></member>
              <member><name>message</name><value><string>(getFullPath) - Parameter nodeid is required, but has not been provided</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act & Assert
        var exception = Assert.Throws<TestLinkValidationException>(() => 
            XmlRpcResponseParser.ParseResponse<string>(errorXml));
        
        Assert.Contains("Parameter nodeid is required", exception.Message);
    }

    [Fact]
    public void ParseResponse_ShouldThrowTestLinkNotFoundException_WhenNotFoundError()
    {
        // Arrange
        var errorXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>code</name><value><int>200</int></value></member>
              <member><name>message</name><value><string>Project not found</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act & Assert
        var exception = Assert.Throws<TestLinkNotFoundException>(() => 
            XmlRpcResponseParser.ParseResponse<string>(errorXml));
        
        Assert.Contains("Project not found", exception.Message);
    }

    [Fact]
    public void ParseResponse_ShouldThrowTestLinkAuthenticationException_WhenAuthError()
    {
        // Arrange
        var errorXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>code</name><value><int>200</int></value></member>
              <member><name>message</name><value><string>Invalid developer key provided</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act & Assert
        var exception = Assert.Throws<TestLinkAuthenticationException>(() => 
            XmlRpcResponseParser.ParseResponse<string>(errorXml));
        
        Assert.Contains("Invalid developer key", exception.Message);
    }

    [Fact]
    public void ParseResponse_ShouldReturnCorrectValue_WhenSuccessfulResponse()
    {
        // Arrange
        var successXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <string>Hello TestLink API</string>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<string>(successXml);

        // Assert
        Assert.Equal("Hello TestLink API", result);
    }

    [Fact]
    public void ParseResponse_ShouldThrowTestLinkApiException_WhenGeneralError()
    {
        // Arrange
        var errorXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>code</name><value><int>500</int></value></member>
              <member><name>message</name><value><string>Internal server error occurred</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act & Assert
        var exception = Assert.Throws<TestLinkApiException>(() => 
            XmlRpcResponseParser.ParseResponse<string>(errorXml));
        
        Assert.Contains("Internal server error occurred", exception.Message);
        Assert.Contains("500", exception.Message);
    }

    [Fact]
    public void ParseResponse_ShouldThrowTestLinkApiException_WhenStandardXmlRpcFault()
    {
        // Arrange - Standard XML-RPC fault format
        var faultXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <fault>
                <value>
                  <struct>
                    <member>
                      <name>faultCode</name>
                      <value><int>404</int></value>
                    </member>
                    <member>
                      <name>faultString</name>
                      <value><string>Method not found</string></value>
                    </member>
                  </struct>
                </value>
              </fault>
            </methodResponse>
            """;

        // Act & Assert
        var exception = Assert.Throws<TestLinkApiException>(() => 
            XmlRpcResponseParser.ParseResponse<string>(faultXml));
        
        Assert.Contains("Method not found", exception.Message);
        Assert.Contains("404", exception.Message);
    }

    [Fact]
    public void ParseResponse_ShouldReturnGeneralResult_WhenCreateTestProjectSuccessResponse()
    {
        // Arrange - The successful XML response from TestLink for CreateTestProject
        var successXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>operation</name><value><string>createTestProject</string></value></member>
              <member><name>additionalInfo</name><value><string></string></value></member>
              <member><name>status</name><value><boolean>1</boolean></value></member>
              <member><name>id</name><value><string>29</string></value></member>
              <member><name>message</name><value><string>Success!</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(successXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("createTestProject", result.Operation);
        Assert.Equal("Success!", result.Message);
        Assert.True(result.Status);
        Assert.Equal(29, result.Id);
        Assert.Null(result.AdditionalInfo); // Should be null since additionalInfo is empty string
    }

    [Fact]
    public void ParseResponse_ShouldParseTestProjectCollection_FromRealTestLinkResponse()
    {
        // Arrange - The actual XML response from TestLink's tl.getProjects method
        var testProjectsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>id</name><value><string>33</string></value></member>
              <member><name>notes</name><value><string></string></value></member>
              <member><name>color</name><value><string></string></value></member>
              <member><name>active</name><value><string>1</string></value></member>
              <member><name>option_reqs</name><value><string>0</string></value></member>
              <member><name>option_priority</name><value><string>0</string></value></member>
              <member><name>option_automation</name><value><string>0</string></value></member>
              <member><name>options</name><value><string>O:8:&quot;stdClass&quot;:4:{s:19:&quot;requirementsEnabled&quot;;i:1;s:19:&quot;testPriorityEnabled&quot;;i:1;s:17:&quot;automationEnabled&quot;;i:1;s:16:&quot;inventoryEnabled&quot;;i:1;}</string></value></member>
              <member><name>prefix</name><value><string>DT4343</string></value></member>
              <member><name>tc_counter</name><value><string>0</string></value></member>
              <member><name>is_public</name><value><string>1</string></value></member>
              <member><name>issue_tracker_enabled</name><value><string>0</string></value></member>
              <member><name>reqmgr_integration_enabled</name><value><string>0</string></value></member>
              <member><name>api_key</name><value><string>8d0fc760a2cf9895e699486e735bf41cb73198b5e7f0e2959a53e2bfa972bfb6</string></value></member>
              <member><name>name</name><value><string>DuplicateTest_1d048f73b17b4c69976b228d244d5c82</string></value></member>
              <member><name>opt</name><value><struct>
              <member><name>requirementsEnabled</name><value><int>1</int></value></member>
              <member><name>testPriorityEnabled</name><value><int>1</int></value></member>
              <member><name>automationEnabled</name><value><int>1</int></value></member>
              <member><name>inventoryEnabled</name><value><int>1</int></value></member>
            </struct></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestProject>>(testProjectsXml);

        // Assert
        Assert.NotNull(result);
        var projects = result.ToList();
        Assert.Single(projects); // Should have exactly one project
        
        var project = projects[0];
        Assert.NotNull(project);
        
        // Verify all the parsed values
        Assert.Equal(33, project.Id);
        Assert.Equal("DuplicateTest_1d048f73b17b4c69976b228d244d5c82", project.Name);
        Assert.Equal("DT4343", project.Prefix);
        Assert.Equal(string.Empty, project.Notes);
        Assert.Equal(string.Empty, project.Color);
        Assert.True(project.Active);
        Assert.Equal(0, project.TestCaseCounter);
        Assert.True(project.IsPublic);
        Assert.False(project.IssueTrackerEnabled);
        Assert.False(project.ReqMgrIntegrationEnabled);
        Assert.Equal("8d0fc760a2cf9895e699486e735bf41cb73198b5e7f0e2959a53e2bfa972bfb6", project.ApiKey);
        
        // Verify options from the 'opt' struct (should take precedence over individual option fields)
        Assert.True(project.RequirementsEnabled);  // From opt.requirementsEnabled = 1
        Assert.True(project.TestPriorityEnabled);  // From opt.testPriorityEnabled = 1
        Assert.True(project.AutomationEnabled);    // From opt.automationEnabled = 1
        Assert.True(project.InventoryEnabled);     // From opt.inventoryEnabled = 1
    }

    [Fact]
    public void ParseResponse_ShouldParseSingleTestProject_FromGetTestProjectByNameResponse()
    {
        // Arrange - Response from tl.getTestProjectByName (returns single project or array with one project)
        var singleProjectXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>id</name><value><string>1</string></value></member>
                      <member><name>name</name><value><string>Test Project</string></value></member>
                      <member><name>prefix</name><value><string>TP</string></value></member>
                      <member><name>notes</name><value><string>Main test project</string></value></member>
                      <member><name>color</name><value><string>#FF0000</string></value></member>
                      <member><name>active</name><value><string>1</string></value></member>
                      <member><name>tc_counter</name><value><string>5</string></value></member>
                      <member><name>is_public</name><value><string>1</string></value></member>
                      <member><name>issue_tracker_enabled</name><value><string>1</string></value></member>
                      <member><name>reqmgr_integration_enabled</name><value><string>0</string></value></member>
                      <member><name>api_key</name><value><string>test-api-key-12345</string></value></member>
                      <member><name>opt</name><value><struct>
                        <member><name>requirementsEnabled</name><value><int>0</int></value></member>
                        <member><name>testPriorityEnabled</name><value><int>1</int></value></member>
                        <member><name>automationEnabled</name><value><int>1</int></value></member>
                        <member><name>inventoryEnabled</name><value><int>0</int></value></member>
                      </struct></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestProject>(singleProjectXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Project", result.Name);
        Assert.Equal("TP", result.Prefix);
        Assert.Equal("Main test project", result.Notes);
        Assert.Equal("#FF0000", result.Color);
        Assert.True(result.Active);
        Assert.Equal(5, result.TestCaseCounter);
        Assert.True(result.IsPublic);
        Assert.True(result.IssueTrackerEnabled);
        Assert.False(result.ReqMgrIntegrationEnabled);
        Assert.Equal("test-api-key-12345", result.ApiKey);
        
        // Verify options from the 'opt' struct
        Assert.False(result.RequirementsEnabled);   // opt.requirementsEnabled = 0
        Assert.True(result.TestPriorityEnabled);    // opt.testPriorityEnabled = 1
        Assert.True(result.AutomationEnabled);      // opt.automationEnabled = 1
        Assert.False(result.InventoryEnabled);      // opt.inventoryEnabled = 0
    }

    [Fact]
    public void ParseResponse_ShouldHandleLegacyOptionsFormat_WhenOptStructNotPresent()
    {
        // Arrange - Legacy format without 'opt' struct (uses individual option fields)
        var legacyProjectXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>id</name><value><string>2</string></value></member>
                      <member><name>name</name><value><string>Legacy Project</string></value></member>
                      <member><name>prefix</name><value><string>LP</string></value></member>
                      <member><name>notes</name><value><string></string></value></member>
                      <member><name>color</name><value><string></string></value></member>
                      <member><name>active</name><value><string>1</string></value></member>
                      <member><name>option_reqs</name><value><string>1</string></value></member>
                      <member><name>option_priority</name><value><string>0</string></value></member>
                      <member><name>option_automation</name><value><string>1</string></value></member>
                      <member><name>tc_counter</name><value><string>3</string></value></member>
                      <member><name>is_public</name><value><string>0</string></value></member>
                      <member><name>issue_tracker_enabled</name><value><string>0</string></value></member>
                      <member><name>reqmgr_integration_enabled</name><value><string>1</string></value></member>
                      <member><name>api_key</name><value><string>legacy-key-67890</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestProject>(legacyProjectXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("Legacy Project", result.Name);
        Assert.Equal("LP", result.Prefix);
        Assert.Equal(string.Empty, result.Notes);
        Assert.Equal(string.Empty, result.Color);
        Assert.True(result.Active);
        Assert.Equal(3, result.TestCaseCounter);
        Assert.False(result.IsPublic);
        Assert.False(result.IssueTrackerEnabled);
        Assert.True(result.ReqMgrIntegrationEnabled);
        Assert.Equal("legacy-key-67890", result.ApiKey);
        
        // Verify options from individual option fields (fallback when no 'opt' struct)
        Assert.True(result.RequirementsEnabled);    // option_reqs = "1"
        Assert.False(result.TestPriorityEnabled);   // option_priority = "0"
        Assert.True(result.AutomationEnabled);      // option_automation = "1"
        Assert.False(result.InventoryEnabled);      // No option_inventory field, defaults to false
    }

    [Fact]
    public void ParseResponse_ShouldHandleEmptyProjectsList_WhenNoProjectsExist()
    {
        // Arrange - Empty array response when no projects exist
        var emptyProjectsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array>
                      <data></data>
                    </array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestProject>>(emptyProjectsXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseResponse_ShouldHandleMultipleTestProjects_InArray()
    {
        // Arrange - Multiple projects in array
        var multipleProjectsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>id</name><value><string>1</string></value></member>
                        <member><name>name</name><value><string>Project One</string></value></member>
                        <member><name>prefix</name><value><string>P1</string></value></member>
                        <member><name>active</name><value><string>1</string></value></member>
                        <member><name>opt</name><value><struct>
                          <member><name>requirementsEnabled</name><value><int>1</int></value></member>
                          <member><name>testPriorityEnabled</name><value><int>0</int></value></member>
                          <member><name>automationEnabled</name><value><int>1</int></value></member>
                          <member><name>inventoryEnabled</name><value><int>0</int></value></member>
                        </struct></value></member>
                      </struct></value>
                      <value><struct>
                        <member><name>id</name><value><string>2</string></value></member>
                        <member><name>name</name><value><string>Project Two</string></value></member>
                        <member><name>prefix</name><value><string>P2</string></value></member>
                        <member><name>active</name><value><string>0</string></value></member>
                        <member><name>opt</name><value><struct>
                          <member><name>requirementsEnabled</name><value><int>0</int></value></member>
                          <member><name>testPriorityEnabled</name><value><int>1</int></value></member>
                          <member><name>automationEnabled</name><value><int>0</int></value></member>
                          <member><name>inventoryEnabled</name><value><int>1</int></value></member>
                        </struct></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestProject>>(multipleProjectsXml);

        // Assert
        Assert.NotNull(result);
        var projects = result.ToList();
        Assert.Equal(2, projects.Count);
        
        // First project
        var project1 = projects[0];
        Assert.Equal(1, project1.Id);
        Assert.Equal("Project One", project1.Name);
        Assert.Equal("P1", project1.Prefix);
        Assert.True(project1.Active);
        Assert.True(project1.RequirementsEnabled);
        Assert.False(project1.TestPriorityEnabled);
        Assert.True(project1.AutomationEnabled);
        Assert.False(project1.InventoryEnabled);
        
        // Second project
        var project2 = projects[1];
        Assert.Equal(2, project2.Id);
        Assert.Equal("Project Two", project2.Name);
        Assert.Equal("P2", project2.Prefix);
        Assert.False(project2.Active);
        Assert.False(project2.RequirementsEnabled);
        Assert.True(project2.TestPriorityEnabled);
        Assert.False(project2.AutomationEnabled);
        Assert.True(project2.InventoryEnabled);
    }

    [Fact]
    public void Debug_ParseResponse_ShouldNotReturnNulls_WhenParsingTestProjectArray()
    {
        // Arrange - Simplified test case to debug the issue
        var simpleProjectXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>id</name><value><string>1</string></value></member>
                        <member><name>name</name><value><string>Test Project</string></value></member>
                        <member><name>prefix</name><value><string>TP</string></value></member>
                        <member><name>active</name><value><string>1</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestProject>>(simpleProjectXml);

        // Assert
        Assert.NotNull(result);
        var projects = result.ToList();
        Assert.Single(projects);
        Assert.NotNull(projects[0]); // This should not be null
        Assert.Equal(1, projects[0].Id);
        Assert.Equal("Test Project", projects[0].Name);
    }

    [Fact]
    public void VerifyFix_ParseResponse_ShouldReturnValidTestProjects_NotNulls()
    {
        // Arrange - Using the exact XML that was causing null arrays
        var testProjectsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>id</name><value><string>33</string></value></member>
                        <member><name>name</name><value><string>DuplicateTest_1d048f73b17b4c69976b228d244d5c82</string></value></member>
                        <member><name>prefix</name><value><string>DT4343</string></value></member>
                        <member><name>active</name><value><string>1</string></value></member>
                        <member><name>opt</name><value><struct>
                          <member><name>requirementsEnabled</name><value><int>1</int></value></member>
                          <member><name>testPriorityEnabled</name><value><int>1</int></value></member>
                          <member><name>automationEnabled</name><value><int>1</int></value></member>
                          <member><name>inventoryEnabled</name><value><int>1</int></value></member>
                        </struct></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestProject>>(testProjectsXml);

        // Assert - This should NOT return an array of nulls anymore
        Assert.NotNull(result);
        var projects = result.ToList();
        Assert.Single(projects);
        
        var project = projects[0];
        Assert.NotNull(project); // This was returning null before the fix
        Assert.Equal(33, project.Id);
        Assert.Equal("DuplicateTest_1d048f73b17b4c69976b228d244d5c82", project.Name);
        Assert.Equal("DT4343", project.Prefix);
        Assert.True(project.Active);
        
        // Verify options are properly parsed from opt struct
        Assert.True(project.RequirementsEnabled);
        Assert.True(project.TestPriorityEnabled);
        Assert.True(project.AutomationEnabled);
        Assert.True(project.InventoryEnabled);
    }

    [Fact]
    public void ParseResponse_ShouldReturnEmptyCollection_WhenNoProjectsExist()
    {
        // Arrange - The error XML response when a project has no test suites (code 7008)
        var emptyProjectXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                      <member><name>code</name><value><int>7008</int></value></member>
                      <member><name>message</name><value><string>(getFirstLevelTestSuitesForTestProject) - Test Project (DuplicateTest_1d048f73b17b4c69976b228d244d5c82) is empty.</string></value></member>
                    </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act - Should return empty collection instead of throwing exception
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestSuite>>(emptyProjectXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseResponse_ShouldReturnEmptyCollection_WhenExactEmptyProjectResponse()
    {
        // Arrange - The exact XML response you provided when a project has no test suites
        var emptyProjectXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>code</name><value><int>7008</int></value></member>
              <member><name>message</name><value><string>(getFirstLevelTestSuitesForTestProject) - Test Project (DuplicateTest_1d048f73b17b4c69976b228d244d5c82) is empty.</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act - Should return empty collection instead of throwing exception
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestSuite>>(emptyProjectXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Should also work with List<TestSuite>
        var listResult = XmlRpcResponseParser.ParseResponse<List<TestSuite>>(emptyProjectXml);
        Assert.NotNull(listResult);
        Assert.Empty(listResult);
    }

    [Fact]
    public void ParseResponse_ShouldStillThrowException_WhenNonEmptyProjectError()
    {
        // Arrange - Other error that should still throw exception
        var errorXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>code</name><value><int>500</int></value></member>
                        <member><name>message</name><value><string>Some other error occurred</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act & Assert - Should still throw exception for non-7008 errors
        var exception = Assert.Throws<TestLinkApiException>(() => 
            XmlRpcResponseParser.ParseResponse<IEnumerable<TestSuite>>(errorXml));
        
        Assert.Contains("Some other error occurred", exception.Message);
        Assert.Contains("500", exception.Message);
    }

    [Fact]
    public void Debug_ParseResponse_ShouldDetectCode7008AsError()
    {
        // Arrange - Minimal test to verify 7008 error detection
        var emptyProjectXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>code</name><value><int>7008</int></value></member>
                        <member><name>message</name><value><string>Test Project is empty.</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act - Should return empty collection for collection types, throw for non-collection types
        var collectionResult = XmlRpcResponseParser.ParseResponse<IEnumerable<TestSuite>>(emptyProjectXml);
        
        // Assert - Collection should be empty
        Assert.NotNull(collectionResult);
        Assert.Empty(collectionResult);
        
        // For non-collection types, should throw exception
        Assert.Throws<TestLinkApiException>(() => 
            XmlRpcResponseParser.ParseResponse<string>(emptyProjectXml));
    }

    [Fact]
    public void ParseResponse_ShouldParseActualGetTestProjectByNameResponse()
    {
        // Arrange - The exact XML response provided by the user for GetTestProjectByName
        var getProjectByNameXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>id</name><value><string>33</string></value></member>
                      <member><name>notes</name><value><string></string></value></member>
                      <member><name>color</name><value><string></string></value></member>
                      <member><name>active</name><value><string>1</string></value></member>
                      <member><name>option_reqs</name><value><string>0</string></value></member>
                      <member><name>option_priority</name><value><string>0</string></value></member>
                      <member><name>option_automation</name><value><string>0</string></value></member>
                      <member><name>options</name><value><string>O:8:&quot;stdClass&quot;:4:{s:19:&quot;requirementsEnabled&quot;;i:1;s:19:&quot;testPriorityEnabled&quot;;i:1;s:17:&quot;automationEnabled&quot;;i:1;s:16:&quot;inventoryEnabled&quot;;i:1;}</string></value></member>
                      <member><name>prefix</name><value><string>DT4343</string></value></member>
                      <member><name>tc_counter</name><value><string>0</string></value></member>
                      <member><name>is_public</name><value><string>1</string></value></member>
                      <member><name>issue_tracker_enabled</name><value><string>0</string></value></member>
                      <member><name>reqmgr_integration_enabled</name><value><string>0</string></value></member>
                      <member><name>api_key</name><value><string>8d0fc760a2cf9895e699486e735bf41cb73198b5e7f0e2959a53e2bfa972bfb6</string></value></member>
                      <member><name>name</name><value><string>DuplicateTest_1d048f73b17b4c69976b228d244d5c82</string></value></member>
                      <member><name>opt</name><value><struct>
                        <member><name>requirementsEnabled</name><value><int>1</int></value></member>
                        <member><name>testPriorityEnabled</name><value><int>1</int></value></member>
                        <member><name>automationEnabled</name><value><int>1</int></value></member>
                        <member><name>inventoryEnabled</name><value><int>1</int></value></member>
                      </struct></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestProject>(getProjectByNameXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(33, result.Id);
        Assert.Equal("DuplicateTest_1d048f73b17b4c69976b228d244d5c82", result.Name);
        Assert.Equal("DT4343", result.Prefix);
        Assert.Equal(string.Empty, result.Notes);
        Assert.Equal(string.Empty, result.Color);
        Assert.True(result.Active);
        Assert.Equal(0, result.TestCaseCounter);
        Assert.True(result.IsPublic);
        Assert.False(result.IssueTrackerEnabled);
        Assert.False(result.ReqMgrIntegrationEnabled);
        Assert.Equal("8d0fc760a2cf9895e699486e735bf41cb73198b5e7f0e2959a53e2bfa972bfb6", result.ApiKey);
        
        // Verify options from the 'opt' struct (should take precedence over individual option fields)
        Assert.True(result.RequirementsEnabled);  // From opt.requirementsEnabled = 1
        Assert.True(result.TestPriorityEnabled);  // From opt.testPriorityEnabled = 1
        Assert.True(result.AutomationEnabled);    // From opt.automationEnabled = 1
        Assert.True(result.InventoryEnabled);     // From opt.inventoryEnabled = 1
    }

    [Fact]
    public void ParseResponse_ShouldReturnEmptyCollection_WhenEmptyStringResponse()
    {
        // Arrange - The exact XML response when TestLink returns empty string for no test plans
        var emptyStringXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <string></string>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act - Should return empty collection for collection types
        var testPlansResult = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlan>>(emptyStringXml);
        var testSuitesResult = XmlRpcResponseParser.ParseResponse<IEnumerable<TestSuite>>(emptyStringXml);
        var testPlansListResult = XmlRpcResponseParser.ParseResponse<List<TestPlan>>(emptyStringXml);

        // Assert
        Assert.NotNull(testPlansResult);
        Assert.Empty(testPlansResult);
        
        Assert.NotNull(testSuitesResult);
        Assert.Empty(testSuitesResult);
        
        Assert.NotNull(testPlansListResult);
        Assert.Empty(testPlansListResult);
        
        // For non-collection types, should return the empty string
        var stringResult = XmlRpcResponseParser.ParseResponse<string>(emptyStringXml);
        Assert.Equal(string.Empty, stringResult);
    }

    [Fact]
    public void ParseResponse_ShouldParseAttachmentRequestResponse_FromRealTestLinkResponse()
    {
        // Arrange - The exact XML response from TestLink's uploadTestProjectAttachment method
        var attachmentResponseXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>fk_id</name><value><int>44</int></value></member>
                      <member><name>fk_table</name><value><string>nodes_hierarchy</string></value></member>
                      <member><name>title</name><value><string>Test Attachment</string></value></member>
                      <member><name>description</name><value><string>Test attachment uploaded by integration test</string></value></member>
                      <member><name>file_name</name><value><string>test-attachment.txt</string></value></member>
                      <member><name>file_size</name><value><int>33</int></value></member>
                      <member><name>file_type</name><value><string>text/plain</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<AttachmentRequestResponse>(attachmentResponseXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(44, result.ForeignKeyId);
        Assert.Equal("nodes_hierarchy", result.LinkedTableName);
        Assert.Equal("Test Attachment", result.Title);
        Assert.Equal("Test attachment uploaded by integration test", result.Description);
        Assert.Equal("test-attachment.txt", result.FileName);
        Assert.Equal(33, result.Size);
        Assert.Equal("text/plain", result.FileType);
    }

    [Fact]
    public void ParseResponse_ShouldParseCreateTestCaseWithDebugHeaders()
    {
        // Arrange - The actual response from TestLink with debug headers before XML
        var responseWithHeaders = """
            ============================================================================== 
            DB Access Error - debug_print_backtrace() OUTPUT START 
            ATTENTION: Enabling more debug info will produce path disclosure weakness (CWE-200) 
                       Having this additional Information could be useful for reporting 
                       issue to development TEAM. 
            ============================================================================== 
            ============================================================================== 
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>operation</name><value><string>createTestCase</string></value></member>
              <member><name>status</name><value><boolean>1</boolean></value></member>
              <member><name>id</name><value><string>63</string></value></member>
              <member><name>additionalInfo</name><value><struct>
              <member><name>id</name><value><string>63</string></value></member>
              <member><name>external_id</name><value><string>7</string></value></member>
              <member><name>status_ok</name><value><int>1</int></value></member>
              <member><name>msg</name><value><string>ok</string></value></member>
              <member><name>new_name</name><value><string></string></value></member>
              <member><name>version_number</name><value><int>1</int></value></member>
              <member><name>has_duplicate</name><value><boolean>0</boolean></value></member>
              <member><name>external_id_already_exists</name><value><boolean>0</boolean></value></member>
              <member><name>update_name</name><value><boolean>0</boolean></value></member>
              <member><name>tcversion_id</name><value><string>64</string></value></member>
            </struct></value></member>
              <member><name>message</name><value><string>Success!</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(responseWithHeaders);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("createTestCase", result.Operation);
        Assert.Equal("Success!", result.Message);
        Assert.True(result.Status);
        Assert.Equal(63, result.Id);
        
        // Verify additional info
        Assert.NotNull(result.AdditionalInfo);
        Assert.Equal(63, result.AdditionalInfo.Id);
        Assert.Equal(7, result.AdditionalInfo.ExternalId);
        Assert.Equal(1, result.AdditionalInfo.VersionNumber);
        Assert.Equal("ok", result.AdditionalInfo.Message);
        Assert.False(result.AdditionalInfo.HasDuplicate);
    }

    [Fact]
    public void ParseResponse_ShouldHandleVariousHeaderFormats()
    {
        // Test with different types of headers before XML
        var responses = new[]
        {
            // Simple pre-XML content
            """
            Some debug text here
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <string>Hello TestLink</string>
                  </value>
                </param>
              </params>
            </methodResponse>
            """,
            
            // Multiple lines with special characters
            """
            =============================================================================
            WARNING: Debug mode enabled
            =============================================================================
            
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <string>Debug Test</string>
                  </value>
                </param>
              </params>
            </methodResponse>
            """,
            
            // No XML declaration, just methodResponse
            """
            Debug headers here
            <methodResponse>
              <params>
                <param>
                  <value>
                    <string>No Declaration</string>
                  </value>
                </param>
              </params>
            </methodResponse>
            """,
            
            // Clean XML (should still work)
            """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <string>Clean XML</string>
                  </value>
                </param>
              </params>
            </methodResponse>
            """
        };

        var expectedResults = new[] { "Hello TestLink", "Debug Test", "No Declaration", "Clean XML" };

        // Act & Assert
        for (int i = 0; i < responses.Length; i++)
        {
            var result = XmlRpcResponseParser.ParseResponse<string>(responses[i]);
            Assert.Equal(expectedResults[i], result);
        }
    }

    [Fact]
    public void ParseResponse_ShouldParseAttachmentsFromStructWithNumericKeys()
    {
        // Arrange - The actual XML response from TestLink's getTestCaseAttachments method
        var attachmentsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
              <member><name>17</name><value><struct>
              <member><name>id</name><value><string>17</string></value></member>
              <member><name>name</name><value><string>test-attachment.txt</string></value></member>
              <member><name>file_type</name><value><string>text/plain</string></value></member>
              <member><name>title</name><value><string>Test Attachment</string></value></member>
              <member><name>date_added</name><value><string>2025-07-18 16:19:49</string></value></member>
              <member><name>content</name><value><string>VGVzdCBhdHRhY2htZW50IGNvbnRlbnQ=</string></value></member>
            </struct></value></member>
            </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<Attachment>>(attachmentsXml);

        // Assert
        Assert.NotNull(result);
        var attachments = result.ToList();
        Assert.Single(attachments);
        
        var attachment = attachments[0];
        Assert.NotNull(attachment);
        Assert.Equal(17, attachment.Id);
        Assert.Equal("test-attachment.txt", attachment.Name);
        Assert.Equal("text/plain", attachment.FileType);
        Assert.Equal("Test Attachment", attachment.Title);
        Assert.Equal(new DateTime(2025, 7, 18, 16, 19, 49), attachment.DateAdded);
        
        // Verify base64 content was decoded
        Assert.NotNull(attachment.Content);
        var expectedContent = "Test attachment content";
        var actualContent = System.Text.Encoding.UTF8.GetString(attachment.Content);
        Assert.Equal(expectedContent, actualContent);
    }

    [Fact]
    public void ParseResponse_ShouldReturnEmptyCollection_WhenNoAttachmentsExist()
    {
        // Arrange - Empty struct response when no attachments exist
        var emptyAttachmentsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct></struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<Attachment>>(emptyAttachmentsXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseResponse_ShouldParseMultipleAttachmentsFromStruct()
    {
        // Arrange - Multiple attachments with numeric keys
        var multipleAttachmentsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
              <member><name>17</name><value><struct>
              <member><name>id</name><value><string>17</string></value></member>
              <member><name>name</name><value><string>first-attachment.txt</string></value></member>
              <member><name>file_type</name><value><string>text/plain</string></value></member>
              <member><name>title</name><value><string>First Attachment</string></value></member>
              <member><name>date_added</name><value><string>2025-07-18 16:19:49</string></value></member>
              <member><name>content</name><value><string>Rmlyc3QgYXR0YWNobWVudA==</string></value></member>
            </struct></value></member>
              <member><name>23</name><value><struct>
              <member><name>id</name><value><string>23</string></value></member>
              <member><name>name</name><value><string>second-attachment.pdf</string></value></member>
              <member><name>file_type</name><value><string>application/pdf</string></value></member>
              <member><name>title</name><value><string>Second Attachment</string></value></member>
              <member><name>date_added</name><value><string>2025-07-18 17:30:15</string></value></member>
              <member><name>content</name><value><string>U2Vjb25kIGF0dGFjaG1lbnQ=</string></value></member>
            </struct></value></member>
            </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<Attachment>>(multipleAttachmentsXml);

        // Assert
        Assert.NotNull(result);
        var attachments = result.ToList();
        Assert.Equal(2, attachments.Count);
        
        // First attachment
        var firstAttachment = attachments.FirstOrDefault(a => a.Id == 17);
        Assert.NotNull(firstAttachment);
        Assert.Equal("first-attachment.txt", firstAttachment.Name);
        Assert.Equal("text/plain", firstAttachment.FileType);
        Assert.Equal("First Attachment", firstAttachment.Title);
        
        // Second attachment
        var secondAttachment = attachments.FirstOrDefault(a => a.Id == 23);
        Assert.NotNull(secondAttachment);
        Assert.Equal("second-attachment.pdf", secondAttachment.Name);
        Assert.Equal("application/pdf", secondAttachment.FileType);
        Assert.Equal("Second Attachment", secondAttachment.Title);
    }

    [Fact]
    public void ParseResponse_ShouldParseTestCase_WithCorrectFieldMapping()
    {
        // Arrange - The exact XML response provided by the user that was causing the duplicate key issue
        var testCaseXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>code</name><value><int>200</int></value></member>
              <member><name>testcase_id</name><value><string>120</string></value></member>
              <member><name>id</name><value><string>121</string></value></member>
              <member><name>node_order</name><value><string>0</string></value></member>
              <member><name>author_id</name><value><string>1</string></value></member>
              <member><name>author_login</name><value><string>admin</string></value></member>
              <member><name>name</name><value><string>VersionedTestCase_dfcfbc3bbc7f486ca4c40a6f1e39dade</string></value></member>
              <member><name>tc_external_id</name><value><string>26</string></value></member>
              <member><name>version</name><value><string>1</string></value></member>
              <member><name>layout</name><value><string>1</string></value></member>
              <member><name>status</name><value><string>1</string></value></member>
              <member><name>summary</name><value><string>Test case for version testing</string></value></member>
              <member><name>preconditions</name><value><string></string></value></member>
              <member><name>importance</name><value><string>2</string></value></member>
              <member><name>creation_ts</name><value><string>2025-07-19 17:57:03</string></value></member>
              <member><name>modification_ts</name><value><string>0000-00-00 00:00:00</string></value></member>
              <member><name>active</name><value><string>1</string></value></member>
              <member><name>is_open</name><value><string>1</string></value></member>
              <member><name>execution_type</name><value><string>1</string></value></member>
              <member><name>estimated_exec_duration</name><value><string></string></value></member>
              <member><name>author_first_name</name><value><string>Testlink</string></value></member>
              <member><name>author_last_name</name><value><string>Administrator</string></value></member>
              <member><name>updater_first_name</name><value><string></string></value></member>
              <member><name>updater_last_name</name><value><string></string></value></member>
              <member><name>steps</name><value><string></string></value></member>
              <member><name>full_tc_external_id</name><value><string>TP-26</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestCase>(testCaseXml);

        // Assert
        Assert.NotNull(result);
        
        // The key issue that was being fixed:
        // - Id should map to "id" (121 in this case) - this is the test case version ID
        // - TestCaseId should map to "testcase_id" (120 in this case) - this is the actual test case ID
        Assert.Equal(121, result.Id);
        Assert.Equal(120, result.TestCaseId); // This was returning 0 before the fix
        
        // Other field mappings
        Assert.Equal("VersionedTestCase_dfcfbc3bbc7f486ca4c40a6f1e39dade", result.Name);
        Assert.Equal("26", result.ExternalId); // Maps from "tc_external_id"
        Assert.Equal("Test case for version testing", result.Summary);
        Assert.Equal(string.Empty, result.Preconditions);
        Assert.Equal(1, result.Version);
        Assert.Equal(0, result.TestSuiteId); // Not present in this response
        Assert.True(result.Active);
        Assert.True(result.IsOpen);
        Assert.Equal(1, result.Status);
        Assert.Equal(2, result.Importance);
        Assert.Equal(1, result.ExecutionType);
        Assert.Equal(1, result.AuthorId);
        Assert.Equal(0, result.UpdaterId); // Empty string should parse to 0
        Assert.Equal("admin", result.AuthorLogin);
        Assert.Null(result.UpdaterLogin);
        Assert.Equal("Testlink", result.AuthorFirstName);
        Assert.Equal("Administrator", result.AuthorLastName);
        Assert.Equal(string.Empty, result.UpdaterFirstName);
        Assert.Equal(string.Empty, result.UpdaterLastName);
        Assert.Equal("1", result.Layout);
        Assert.Equal(0, result.NodeOrder);
        
        // Verify dates
        Assert.Equal(new DateTime(2025, 7, 19, 17, 57, 3), result.CreationTimestamp);
        // ModificationTimestamp should handle the invalid date format gracefully
        Assert.Equal(default(DateTime), result.ModificationTimestamp);
        
        // Steps should be empty since it was an empty string in the response
        Assert.NotNull(result.Steps);
        Assert.Empty(result.Steps);
    }

    [Fact]
    public void ParseResponse_ShouldParseTestCaseWithSteps_WhenStepsAreProvided()
    {
        // Arrange - TestCase response with actual test steps
        var testCaseWithStepsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>id</name><value><string>122</string></value></member>
              <member><name>testcase_id</name><value><string>121</string></value></member>
              <member><name>name</name><value><string>TestCaseWithSteps</string></value></member>
              <member><name>summary</name><value><string>Test case with test steps</string></value></member>
              <member><name>version</name><value><string>1</string></value></member>
              <member><name>testsuite_id</name><value><string>50</string></value></member>
              <member><name>tc_external_id</name><value><string>27</string></value></member>
              <member><name>active</name><value><string>1</string></value></member>
              <member><name>steps</name><value><array><data>
                <value><struct>
                  <member><name>step_number</name><value><string>1</string></value></member>
                  <member><name>actions</name><value><string>Open the application</string></value></member>
                  <member><name>expected_results</name><value><string>Application opens successfully</string></value></member>
                </struct></value>
                <value><struct>
                  <member><name>step_number</name><value><string>2</string></value></member>
                  <member><name>actions</name><value><string>Login with valid credentials</string></value></member>
                  <member><name>expected_results</name><value><string>User is logged in</string></value></member>
                </struct></value>
              </data></array></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestCase>(testCaseWithStepsXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(122, result.Id);
        Assert.Equal(121, result.TestCaseId);
        Assert.Equal("TestCaseWithSteps", result.Name);
        Assert.Equal("Test case with test steps", result.Summary);
        
        // Verify steps were parsed correctly
        Assert.NotNull(result.Steps);
        Assert.Equal(2, result.Steps.Count);
        
        // First step
        var step1 = result.Steps[0];
        Assert.Equal(1, step1.StepNumber);
        Assert.Equal("Open the application", step1.Actions);
        Assert.Equal("Application opens successfully", step1.ExpectedResults);
        
        // Second step
        var step2 = result.Steps[1];
        Assert.Equal(2, step2.StepNumber);
        Assert.Equal("Login with valid credentials", step2.Actions);
        Assert.Equal("User is logged in", step2.ExpectedResults);
    }

    [Fact]
    public void ParseResponse_ShouldParseTestCaseFromTestSuite_WithCorrectFieldMapping()
    {
        // Arrange - The exact XML response provided by the user from GetTestCasesForTestSuite
        var testCaseFromTestSuiteXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>id</name><value><string>51</string></value></member>
              <member><name>parent_id</name><value><string>50</string></value></member>
              <member><name>node_type_id</name><value><string>3</string></value></member>
              <member><name>node_order</name><value><string>0</string></value></member>
              <member><name>node_table</name><value><string>testcases</string></value></member>
              <member><name>name</name><value><string>TestCaseWithSteps_a6fa04106fad4d54b93434c05c1d6b8c</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestCaseFromTestSuite>>(testCaseFromTestSuiteXml);

        // Assert
        Assert.NotNull(result);
        var testCases = result.ToList();
        Assert.Single(testCases);
        
        var testCase = testCases[0];
        Assert.NotNull(testCase);
        
        // Verify the field mappings from the XML response
        Assert.Equal(51, testCase.Id);
        Assert.Equal("TestCaseWithSteps_a6fa04106fad4d54b93434c05c1d6b8c", testCase.Name);
        Assert.Equal(50, testCase.ParentId); // parent_id maps to ParentId
        Assert.Equal(3, testCase.NodeTypeId); // node_type_id maps to NodeTypeId (3 = testcase)
        Assert.Equal(0, testCase.NodeOrder); // node_order maps to NodeOrder
        Assert.Equal("testcases", testCase.NodeTable); // node_table maps to NodeTable
    }

    [Fact]
    public void ParseResponse_ShouldReturnEmptyCollection_WhenNoTestCasesInTestSuite()
    {
        // Arrange - Empty response when test suite has no test cases
        var emptyTestCasesXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array>
                      <data></data>
                    </array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestCaseFromTestSuite>>(emptyTestCasesXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseResponse_ShouldParseMultipleTestCasesFromTestSuite()
    {
        // Arrange - Multiple test cases in the response
        var multipleTestCasesXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>id</name><value><string>51</string></value></member>
              <member><name>parent_id</name><value><string>50</string></value></member>
              <member><name>node_type_id</name><value><string>3</string></value></member>
              <member><name>node_order</name><value><string>0</string></value></member>
              <member><name>node_table</name><value><string>testcases</string></value></member>
              <member><name>name</name><value><string>FirstTestCase</string></value></member>
            </struct></value>
              <value><struct>
              <member><name>id</name><value><string>52</string></value></member>
              <member><name>parent_id</name><value><string>50</string></value></member>
              <member><name>node_type_id</name><value><string>3</string></value></member>
              <member><name>node_order</name><value><string>1</string></value></member>
              <member><name>node_table</name><value><string>testcases</string></value></member>
              <member><name>name</name><value><string>SecondTestCase</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestCaseFromTestSuite>>(multipleTestCasesXml);

        // Assert
        Assert.NotNull(result);
        var testCases = result.ToList();
        Assert.Equal(2, testCases.Count);
        
        // First test case
        var firstTestCase = testCases[0];
        Assert.Equal(51, firstTestCase.Id);
        Assert.Equal("FirstTestCase", firstTestCase.Name);
        Assert.Equal(0, firstTestCase.NodeOrder);
        Assert.Equal(50, firstTestCase.ParentId);
        
        // Second test case
        var secondTestCase = testCases[1];
        Assert.Equal(52, secondTestCase.Id);
        Assert.Equal("SecondTestCase", secondTestCase.Name);
        Assert.Equal(1, secondTestCase.NodeOrder);
        Assert.Equal(50, secondTestCase.ParentId);
    }

    [Fact]
    public void ParseResponse_ShouldParseTestPlan_WithIsOpenFieldMapping()
    {
        // Arrange - The exact XML response from the user's error that was causing the duplicate key issue
        var testPlanXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>id</name><value><string>185</string></value></member>
              <member><name>testproject_id</name><value><string>155</string></value></member>
              <member><name>notes</name><value><string></string></value></member>
              <member><name>active</name><value><string>1</string></value></member>
              <member><name>is_open</name><value><string>1</string></value></member>
              <member><name>is_public</name><value><string>1</string></value></member>
              <member><name>api_key</name><value><string>30767343d08fd0fde8b1d5025af88b714fc51877d2b746cd1d962ea5077da885</string></value></member>
              <member><name>name</name><value><string>TestPlan_23222b46169442a7af024ead9fc83a61</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act - This should no longer throw the duplicate key exception
        var result = XmlRpcResponseParser.ParseResponse<TestPlan>(testPlanXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(185, result.Id);
        Assert.Equal("TestPlan_23222b46169442a7af024ead9fc83a61", result.Name);
        Assert.Equal(155, result.TestProjectId);
        Assert.Equal(string.Empty, result.Notes);
        Assert.True(result.Active);
        Assert.True(result.IsOpen); // Should correctly map from "is_open" field
        Assert.True(result.IsPublic);
    }

    [Fact]
    public void ParseResponse_ShouldParseTestCase_WithIsOpenFieldMapping()
    {
        // Arrange - TestCase response with "is_open" field to verify TestCase IsOpen mapping still works
        var testCaseXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>id</name><value><string>121</string></value></member>
              <member><name>testcase_id</name><value><string>120</string></value></member>
              <member><name>name</name><value><string>TestCase</string></value></member>
              <member><name>is_open</name><value><string>1</string></value></member>
              <member><name>active</name><value><string>1</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestCase>(testCaseXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(121, result.Id);
        Assert.Equal(120, result.TestCaseId);
        Assert.Equal("TestCase", result.Name);
        Assert.True(result.IsOpen); // Should correctly map from "is_open" field
        Assert.True(result.Active);
    }

    [Fact]
    public void ParseResponse_ShouldParseTestPlan_WithOpenFieldMapping()
    {
        // Arrange - TestPlan response with "open" field (alternative mapping)
        var testPlanXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>id</name><value><string>100</string></value></member>
                      <member><name>name</name><value><string>TestPlan with Open Field</string></value></member>
                      <member><name>testproject_id</name><value><string>50</string></value></member>
                      <member><name>open</name><value><string>1</string></value></member>
                      <member><name>active</name><value><string>1</string></value></member>
                      <member><name>is_public</name><value><string>0</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestPlan>(testPlanXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Id);
        Assert.Equal("TestPlan with Open Field", result.Name);
        Assert.Equal(50, result.TestProjectId);
        Assert.True(result.IsOpen); // Should correctly map from "open" field
        Assert.True(result.Active);
        Assert.False(result.IsPublic);
    }

    [Fact]
    public void ParseResponse_ShouldReturnEmptyCollection_WhenGetTotalsForTestPlanHasNoExecutions()
    {
        // Arrange - The exact XML response that was causing the JSON conversion error
        var emptyTotalsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
              <member><name>with_tester</name><value><array><data>
            </data></array></value></member>
              <member><name>total</name><value><array><data>
            </data></array></value></member>
              <member><name>platforms</name><value><string></string></value></member>
            </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act - This should no longer throw the JSON conversion exception
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlanTotal>>(emptyTotalsXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result); // Should return empty collection when no executions exist
    }

    [Fact]
    public void ParseResponse_ShouldReturnEmptyList_WhenGetTotalsForTestPlanHasNoExecutionsAsList()
    {
        // Arrange - Same XML but expecting List<TestPlanTotal>
        var emptyTotalsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
              <member><name>with_tester</name><value><array><data>
            </data></array></value></member>
              <member><name>total</name><value><array><data>
            </data></array></value></member>
              <member><name>platforms</name><value><string></string></value></member>
            </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<List<TestPlanTotal>>(emptyTotalsXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseResponse_ShouldParseTestPlanTotals_WhenExecutionsExist()
    {
        // Arrange - Normal TestPlanTotal response when executions exist
        var testPlanTotalsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>type</name><value><string>platform</string></value></member>
              <member><name>name</name><value><string>Default Platform</string></value></member>
              <member><name>total_tc</name><value><int>5</int></value></member>
              <member><name>details</name><value><struct>
                <member><name>passed</name><value><int>3</int></value></member>
                <member><name>failed</name><value><int>1</int></value></member>
                <member><name>blocked</name><value><int>1</int></value></member>
                <member><name>not_run</name><value><int>0</int></value></member>
              </struct></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlanTotal>>(testPlanTotalsXml);

        // Assert
        Assert.NotNull(result);
        var totals = result.ToList();
        Assert.Single(totals);
        
        var total = totals[0];
        Assert.Equal("platform", total.Type);
        Assert.Equal("Default Platform", total.Name);
        Assert.Equal(5, total.TotalTestCases);
        
        Assert.NotNull(total.Details);
        Assert.Equal(4, total.Details.Count);
        Assert.Equal(3, total.Details["passed"]);
        Assert.Equal(1, total.Details["failed"]);
        Assert.Equal(1, total.Details["blocked"]);
        Assert.Equal(0, total.Details["not_run"]);
    }
    [Fact]
    public void ParseResponse_ShouldNotThrowJsonException_WhenTestPlanHasNoExecutions()
    {
        // Arrange - The exact XML response that was causing the JSON conversion error
        var problematicXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
              <member><name>with_tester</name><value><array><data>
            </data></array></value></member>
              <member><name>total</name><value><array><data>
            </data></array></value></member>
              <member><name>platforms</name><value><string></string></value></member>
            </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act & Assert - This should no longer throw System.Text.Json.JsonException
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlanTotal>>(problematicXml);

        // Verify the result
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseResponse_ShouldHandleVariationsOfEmptyTotalsResponse()
    {
        // Test various possible empty responses for getTotalsForTestPlan

        // Case 1: Only with_tester field
        var xml1 = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>with_tester</name><value><array><data></data></array></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        var result1 = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlanTotal>>(xml1);
        Assert.NotNull(result1);
        Assert.Empty(result1);

        // Case 2: Only total field
        var xml2 = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>total</name><value><array><data></data></array></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        var result2 = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlanTotal>>(xml2);
        Assert.NotNull(result2);
        Assert.Empty(result2);

        // Case 3: Only platforms field
        var xml3 = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>platforms</name><value><string></string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        var result3 = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlanTotal>>(xml3);
        Assert.NotNull(result3);
        Assert.Empty(result3);
    }

    [Fact]
    public void ParseResponse_ShouldStillWorkForNormalTestPlanTotals()
    {
        // Ensure that normal TestPlanTotal responses still work after our fix
        var normalTotalsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>type</name><value><string>summary</string></value></member>
              <member><name>total_tc</name><value><int>10</int></value></member>
              <member><name>details</name><value><struct>
                <member><name>passed</name><value><int>7</int></value></member>
                <member><name>failed</name><value><int>2</int></value></member>
                <member><name>blocked</name><value><int>1</int></value></member>
              </struct></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlanTotal>>(normalTotalsXml);

        // Assert
        Assert.NotNull(result);
        var totals = result.ToList();
        Assert.Single(totals);

        var total = totals[0];
        Assert.Equal("summary", total.Type);
        Assert.Equal(10, total.TotalTestCases);
        Assert.Equal(3, total.Details.Count);
        Assert.Equal(7, total.Details["passed"]);
        Assert.Equal(2, total.Details["failed"]);
        Assert.Equal(1, total.Details["blocked"]);
    }

    [Fact]
    public void ParseResponse_ShouldOnlyApplySpecialHandling_ToTestPlanTotalCollections()
    {
        // Verify that the special handling only applies to IEnumerable<TestPlanTotal>
        // and not to other collection types that might have similar field names

        var xmlWithSimilarFields = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>total</name><value><string>some_value</string></value></member>
                      <member><name>other_field</name><value><string>other_value</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // This should not return empty collection for non-TestPlanTotal types
        var result = XmlRpcResponseParser.ParseResponse<Dictionary<string, object>>(xmlWithSimilarFields);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("total", result.Keys);
    }
    [Fact]
    public void ParseResponse_ShouldNotThrowDuplicateKeyException_WhenParsingTestPlan()
    {
        // Arrange - The exact XML response that was causing the duplicate key exception
        var testPlanXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
              <value><struct>
              <member><name>id</name><value><string>185</string></value></member>
              <member><name>testproject_id</name><value><string>155</string></value></member>
              <member><name>notes</name><value><string></string></value></member>
              <member><name>active</name><value><string>1</string></value></member>
              <member><name>is_open</name><value><string>1</string></value></member>
              <member><name>is_public</name><value><string>1</string></value></member>
              <member><name>api_key</name><value><string>30767343d08fd0fde8b1d5025af88b714fc51877d2b746cd1d962ea5077da885</string></value></member>
              <member><name>name</name><value><string>TestPlan_23222b46169442a7af024ead9fc83a61</string></value></member>
            </struct></value>
            </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act & Assert - This should no longer throw an exception
        var result = XmlRpcResponseParser.ParseResponse<TestPlan>(testPlanXml);

        // Verify the parsed result
        Assert.NotNull(result);
        Assert.Equal(185, result.Id);
        Assert.Equal("TestPlan_23222b46169442a7af024ead9fc83a61", result.Name);
        Assert.Equal(155, result.TestProjectId);
        Assert.Equal(string.Empty, result.Notes);
        Assert.True(result.Active);
        Assert.True(result.IsOpen); // Should correctly map from "is_open" field
        Assert.True(result.IsPublic);
    }

    [Fact]
    public void FindDictionaryKey_ShouldHandleIsOpenMapping_ForBothTestCaseAndTestPlan()
    {
        // Test with TestCase - should prefer "is_open" when both are present
        var testCaseDict = new Dictionary<string, object?>
        {
            { "is_open", "1" },
            { "open", "0" }  // This should not be selected for TestCase scenarios
        };

        // For TestCase context, it should find "is_open"
        var result = XmlRpcResponseParser.ParseResponse<TestCase>(CreateXmlResponse(testCaseDict));
        Assert.True(result.IsOpen);

        // Test with TestPlan - should handle "is_open"
        var testPlanDict = new Dictionary<string, object?>
        {
            { "id", "100" },
            { "name", "Test Plan" },
            { "testproject_id", "50" },
            { "is_open", "1" },
            { "active", "1" },
            { "is_public", "1" }
        };

        var testPlanResult = XmlRpcResponseParser.ParseResponse<TestPlan>(CreateXmlResponse(testPlanDict));
        Assert.True(testPlanResult.IsOpen);

        // Test with TestPlan - should also handle "open" field
        var testPlanDictWithOpen = new Dictionary<string, object?>
        {
            { "id", "101" },
            { "name", "Test Plan with Open" },
            { "testproject_id", "51" },
            { "open", "1" },
            { "active", "1" },
            { "is_public", "0" }
        };

        var testPlanResultWithOpen = XmlRpcResponseParser.ParseResponse<TestPlan>(CreateXmlResponse(testPlanDictWithOpen));
        Assert.True(testPlanResultWithOpen.IsOpen);
    }

    private static string CreateXmlResponse(Dictionary<string, object?> data)
    {
        var members = string.Join("", data.Select(kvp =>
            $"<member><name>{kvp.Key}</name><value><string>{kvp.Value}</string></value></member>"));

        return $"""
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      {members}
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;
    }
}