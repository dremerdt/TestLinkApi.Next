using TestLinkApi.Next;
using TestLinkApi.Next.Models;
using Xunit;

namespace TestLinkApi.Next.Tests;

public class XmlRpcResponseParserCoreTests
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
    public void ParseResponse_ShouldReturnEmptyCollection_WhenEmptyProjectsExist()
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
}