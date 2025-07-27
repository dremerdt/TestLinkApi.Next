using TestLinkApi.Next;
using TestLinkApi.Next.Models;
using Xunit;

namespace TestLinkApi.Next.Tests;

public class XmlRpcResponseParserTestSuiteTests
{
    [Fact]
    public void ParseResponse_ShouldReturnCollection_WhenChildSuitExist()
    {
        // Arrange - The XML response for parent suite with a child
        var suitsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
              <member><name>id</name><value><string>229</string></value></member>
              <member><name>details</name><value><string></string></value></member>
              <member><name>name</name><value><string>Test Suite 01-01</string></value></member>
              <member><name>node_type_id</name><value><string>2</string></value></member>
              <member><name>node_order</name><value><string>1</string></value></member>
              <member><name>parent_id</name><value><string>8</string></value></member>
            </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act - Should return empty collection instead of throwing exception
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestSuite>>(suitsXml);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
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
    public void ParseResponse_ShouldHandleEmptyTestSuitesList_WhenNoTestSuitesExist()
    {
        // Arrange - Empty array response when no test suites exist
        var emptyTestSuitesXml = """
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
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestSuite>>(emptyTestSuitesXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseResponse_ShouldParseMultipleTestSuites_InArray()
    {
        // Arrange - Multiple test suites in array
        var multipleTestSuitesXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>id</name><value><string>1</string></value></member>
                        <member><name>name</name><value><string>Test Suite One</string></value></member>
                        <member><name>details</name><value><string>First test suite</string></value></member>
                        <member><name>node_type_id</name><value><string>2</string></value></member>
                        <member><name>node_order</name><value><string>1</string></value></member>
                        <member><name>parent_id</name><value><string>10</string></value></member>
                      </struct></value>
                      <value><struct>
                        <member><name>id</name><value><string>2</string></value></member>
                        <member><name>name</name><value><string>Test Suite Two</string></value></member>
                        <member><name>details</name><value><string>Second test suite</string></value></member>
                        <member><name>node_type_id</name><value><string>2</string></value></member>
                        <member><name>node_order</name><value><string>2</string></value></member>
                        <member><name>parent_id</name><value><string>10</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestSuite>>(multipleTestSuitesXml);

        // Assert
        Assert.NotNull(result);
        var testSuites = result.ToList();
        Assert.Equal(2, testSuites.Count);
        
        // First test suite
        var testSuite1 = testSuites[0];
        Assert.Equal(1, testSuite1.Id);
        Assert.Equal("Test Suite One", testSuite1.Name);
        Assert.Equal("First test suite", testSuite1.Details);
        Assert.Equal(2, testSuite1.NodeTypeId);
        Assert.Equal(1, testSuite1.NodeOrder);
        Assert.Equal(10, testSuite1.ParentId);
        
        // Second test suite
        var testSuite2 = testSuites[1];
        Assert.Equal(2, testSuite2.Id);
        Assert.Equal("Test Suite Two", testSuite2.Name);
        Assert.Equal("Second test suite", testSuite2.Details);
        Assert.Equal(2, testSuite2.NodeTypeId);
        Assert.Equal(2, testSuite2.NodeOrder);
        Assert.Equal(10, testSuite2.ParentId);
    }

    [Fact]
    public void ParseResponse_ShouldParseSingleTestSuite_FromGetTestSuiteByIdResponse()
    {
        // Arrange - Response from tl.getTestSuiteById (returns single test suite)
        var singleTestSuiteXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>id</name><value><string>100</string></value></member>
                      <member><name>name</name><value><string>Main Test Suite</string></value></member>
                      <member><name>details</name><value><string>Primary test suite for the project</string></value></member>
                      <member><name>node_type_id</name><value><string>2</string></value></member>
                      <member><name>node_order</name><value><string>0</string></value></member>
                      <member><name>parent_id</name><value><string>50</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestSuite>(singleTestSuiteXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Id);
        Assert.Equal("Main Test Suite", result.Name);
        Assert.Equal("Primary test suite for the project", result.Details);
        Assert.Equal(2, result.NodeTypeId);
        Assert.Equal(0, result.NodeOrder);
        Assert.Equal(50, result.ParentId);
    }

    [Fact]
    public void ParseResponse_ShouldHandleTestSuite_WithMinimalFields()
    {
        // Arrange - TestSuite with only required fields
        var minimalTestSuiteXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>id</name><value><string>42</string></value></member>
                      <member><name>name</name><value><string>Minimal Test Suite</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestSuite>(minimalTestSuiteXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Minimal Test Suite", result.Name);
        Assert.Null(result.Details); // When field is missing from XML, parser returns null
        Assert.Equal(0, result.NodeTypeId); // Should default to 0
        Assert.Equal(0, result.NodeOrder); // Should default to 0
        Assert.Equal(0, result.ParentId); // Should default to 0
    }

    [Fact]
    public void ParseResponse_ShouldHandleTestSuite_WithAllFields()
    {
        // Arrange - TestSuite with all possible fields
        var completeTestSuiteXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>id</name><value><string>123</string></value></member>
                      <member><name>name</name><value><string>Complete Test Suite</string></value></member>
                      <member><name>details</name><value><string>This is a complete test suite with all fields</string></value></member>
                      <member><name>node_type_id</name><value><string>2</string></value></member>
                      <member><name>node_order</name><value><string>5</string></value></member>
                      <member><name>parent_id</name><value><string>75</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<TestSuite>(completeTestSuiteXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(123, result.Id);
        Assert.Equal("Complete Test Suite", result.Name);
        Assert.Equal("This is a complete test suite with all fields", result.Details);
        Assert.Equal(2, result.NodeTypeId);
        Assert.Equal(5, result.NodeOrder);
        Assert.Equal(75, result.ParentId);
    }

    [Fact]
    public void ParseResponse_ShouldHandleNestedTestSuites_FromGetTestSuitesForTestSuiteResponse()
    {
        // Arrange - Response from tl.getTestSuitesForTestSuite showing nested structure
        var nestedTestSuitesXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>10</name><value><struct>
                        <member><name>id</name><value><string>10</string></value></member>
                        <member><name>name</name><value><string>Parent Test Suite</string></value></member>
                        <member><name>details</name><value><string>Parent suite details</string></value></member>
                        <member><name>node_type_id</name><value><string>2</string></value></member>
                        <member><name>node_order</name><value><string>1</string></value></member>
                        <member><name>parent_id</name><value><string>5</string></value></member>
                      </struct></value></member>
                      <member><name>11</name><value><struct>
                        <member><name>id</name><value><string>11</string></value></member>
                        <member><name>name</name><value><string>Child Test Suite</string></value></member>
                        <member><name>details</name><value><string>Child suite details</string></value></member>
                        <member><name>node_type_id</name><value><string>2</string></value></member>
                        <member><name>node_order</name><value><string>2</string></value></member>
                        <member><name>parent_id</name><value><string>10</string></value></member>
                      </struct></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestSuite>>(nestedTestSuitesXml);

        // Assert
        Assert.NotNull(result);
        var testSuites = result.ToList();
        Assert.Equal(2, testSuites.Count);
        
        // Parent test suite
        var parentSuite = testSuites.FirstOrDefault(s => s.Id == 10);
        Assert.NotNull(parentSuite);
        Assert.Equal("Parent Test Suite", parentSuite.Name);
        Assert.Equal("Parent suite details", parentSuite.Details);
        Assert.Equal(5, parentSuite.ParentId);
        
        // Child test suite
        var childSuite = testSuites.FirstOrDefault(s => s.Id == 11);
        Assert.NotNull(childSuite);
        Assert.Equal("Child Test Suite", childSuite.Name);
        Assert.Equal("Child suite details", childSuite.Details);
        Assert.Equal(10, childSuite.ParentId); // Should reference the parent suite
    }
}