using TestLinkApi.Next;
using TestLinkApi.Next.Models;
using Xunit;

namespace TestLinkApi.Next.Tests;

public class XmlRpcResponseParserTestPlanTests
{
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
    public void FindDictionaryKey_ShouldHandleIsOpenMapping_ForTestPlan()
    {
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

    [Fact]
    public void ParseResponse_ShouldHandleMultipleTestPlans_InArray()
    {
        // Arrange - Multiple test plans in array
        var multipleTestPlansXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>id</name><value><string>1</string></value></member>
                        <member><name>name</name><value><string>Test Plan One</string></value></member>
                        <member><name>testproject_id</name><value><string>10</string></value></member>
                        <member><name>is_open</name><value><string>1</string></value></member>
                        <member><name>active</name><value><string>1</string></value></member>
                        <member><name>is_public</name><value><string>1</string></value></member>
                        <member><name>notes</name><value><string>First test plan</string></value></member>
                      </struct></value>
                      <value><struct>
                        <member><name>id</name><value><string>2</string></value></member>
                        <member><name>name</name><value><string>Test Plan Two</string></value></member>
                        <member><name>testproject_id</name><value><string>20</string></value></member>
                        <member><name>open</name><value><string>0</string></value></member>
                        <member><name>active</name><value><string>0</string></value></member>
                        <member><name>is_public</name><value><string>0</string></value></member>
                        <member><name>notes</name><value><string>Second test plan</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlan>>(multipleTestPlansXml);

        // Assert
        Assert.NotNull(result);
        var testPlans = result.ToList();
        Assert.Equal(2, testPlans.Count);
        
        // First test plan
        var testPlan1 = testPlans[0];
        Assert.Equal(1, testPlan1.Id);
        Assert.Equal("Test Plan One", testPlan1.Name);
        Assert.Equal(10, testPlan1.TestProjectId);
        Assert.True(testPlan1.IsOpen);
        Assert.True(testPlan1.Active);
        Assert.True(testPlan1.IsPublic);
        Assert.Equal("First test plan", testPlan1.Notes);
        
        // Second test plan
        var testPlan2 = testPlans[1];
        Assert.Equal(2, testPlan2.Id);
        Assert.Equal("Test Plan Two", testPlan2.Name);
        Assert.Equal(20, testPlan2.TestProjectId);
        Assert.False(testPlan2.IsOpen);
        Assert.False(testPlan2.Active);
        Assert.False(testPlan2.IsPublic);
        Assert.Equal("Second test plan", testPlan2.Notes);
    }

    [Fact]
    public void ParseResponse_ShouldHandleEmptyTestPlansList_WhenNoTestPlansExist()
    {
        // Arrange - Empty array response when no test plans exist
        var emptyTestPlansXml = """
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
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlan>>(emptyTestPlansXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseResponse_ShouldParseSingleTestPlan_FromGetTestPlanByNameResponse()
    {
        // Arrange - Response from tl.getTestPlanByName (returns single test plan)
        var singleTestPlanXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>id</name><value><string>42</string></value></member>
                        <member><name>name</name><value><string>Main Test Plan</string></value></member>
                        <member><name>testproject_id</name><value><string>15</string></value></member>
                        <member><name>notes</name><value><string>Primary test plan for the project</string></value></member>
                        <member><name>active</name><value><string>1</string></value></member>
                        <member><name>is_open</name><value><string>1</string></value></member>
                        <member><name>is_public</name><value><string>0</string></value></member>
                        <member><name>api_key</name><value><string>test-plan-api-key-12345</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestPlan>(singleTestPlanXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Main Test Plan", result.Name);
        Assert.Equal(15, result.TestProjectId);
        Assert.Equal("Primary test plan for the project", result.Notes);
        Assert.True(result.Active);
        Assert.True(result.IsOpen);
        Assert.False(result.IsPublic);
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