using TestLinkApi.Next;
using TestLinkApi.Next.Models;
using Xunit;

namespace TestLinkApi.Next.Tests;

public class XmlRpcResponseParserTestCaseTests
{
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
    public void ParseResponse_ShouldHandleTestCase_WithMinimalFields()
    {
        // Arrange - TestCase with only required fields
        var minimalTestCaseXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>id</name><value><string>99</string></value></member>
                        <member><name>testcase_id</name><value><string>98</string></value></member>
                        <member><name>name</name><value><string>Minimal Test Case</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestCase>(minimalTestCaseXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(99, result.Id);
        Assert.Equal(98, result.TestCaseId);
        Assert.Equal("Minimal Test Case", result.Name);
        Assert.Null(result.Summary); // When field is missing from XML, parser returns null
        Assert.Equal(0, result.Version); // Should default to 0
        Assert.False(result.Active); // Should default to false
        Assert.False(result.IsOpen); // Should default to false
    }

    [Fact]
    public void FindDictionaryKey_ShouldHandleIsOpenMapping_ForTestCase()
    {
        // Test with TestCase - should prefer "is_open" when both are present
        var testCaseDict = new Dictionary<string, object?>
        {
            { "id", "121" },
            { "testcase_id", "120" },
            { "name", "Test Case" },
            { "is_open", "1" },
            { "open", "0" }  // This should not be selected for TestCase scenarios
        };

        // For TestCase context, it should find "is_open"
        var result = XmlRpcResponseParser.ParseResponse<TestCase>(CreateXmlResponse(testCaseDict));
        Assert.True(result.IsOpen);
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
                    <array><data>
                      <value><struct>
                        {members}
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;
    }
}