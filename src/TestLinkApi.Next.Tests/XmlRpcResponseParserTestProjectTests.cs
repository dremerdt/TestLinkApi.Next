using TestLinkApi.Next;
using TestLinkApi.Next.Models;
using Xunit;

namespace TestLinkApi.Next.Tests;

public class XmlRpcResponseParserTestProjectTests
{
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
}