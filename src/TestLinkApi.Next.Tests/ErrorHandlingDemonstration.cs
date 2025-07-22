using TestLinkApi.Next;
using Xunit;

namespace TestLinkApi.Next.Tests;

/// <summary>
/// Demonstrates the improved error handling capabilities of the XmlRpcResponseParser
/// </summary>
public class ErrorHandlingDemonstration
{
    [Fact]
    public void DemonstrateOriginalIssue_ParameterRequiredError()
    {
        // This XML is exactly what the user reported receiving from TestLink
        var originalErrorXml = """
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

        // Before our improvement, this would have been parsed as a successful response
        // Now it correctly throws a TestLinkValidationException
        var exception = Assert.Throws<TestLinkValidationException>(() => 
            XmlRpcResponseParser.ParseResponse<string>(originalErrorXml));
        
        Assert.Contains("Parameter nodeid is required", exception.Message);
        Assert.Contains("but has not been provided", exception.Message);
    }

    [Fact]
    public void DemonstrateVariousErrorTypes()
    {
        // Missing developer key
        var authError = """
            <methodResponse>
              <params><param><value><array><data>
                <value><struct>
                  <member><name>code</name><value><int>200</int></value></member>
                  <member><name>message</name><value><string>Invalid developer key</string></value></member>
                </struct></value>
              </data></array></value></param></params>
            </methodResponse>
            """;

        Assert.Throws<TestLinkAuthenticationException>(() => 
            XmlRpcResponseParser.ParseResponse<string>(authError));

        // Resource not found
        var notFoundError = """
            <methodResponse>
              <params><param><value><array><data>
                <value><struct>
                  <member><name>code</name><value><int>200</int></value></member>
                  <member><name>message</name><value><string>Test project not found</string></value></member>
                </struct></value>
              </data></array></value></param></params>
            </methodResponse>
            """;

        Assert.Throws<TestLinkNotFoundException>(() => 
            XmlRpcResponseParser.ParseResponse<string>(notFoundError));

        // Invalid parameter value
        var validationError = """
            <methodResponse>
              <params><param><value><array><data>
                <value><struct>
                  <member><name>code</name><value><int>200</int></value></member>
                  <member><name>message</name><value><string>Invalid project ID provided</string></value></member>
                </struct></value>
              </data></array></value></param></params>
            </methodResponse>
            """;

        Assert.Throws<TestLinkValidationException>(() => 
            XmlRpcResponseParser.ParseResponse<string>(validationError));
    }

    [Fact]
    public void DemonstrateSuccessfulResponseStillWorks()
    {
        // Verify that normal successful responses continue to work as expected
        var successXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <string>TestLink API is working correctly</string>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        var result = XmlRpcResponseParser.ParseResponse<string>(successXml);
        Assert.Equal("TestLink API is working correctly", result);
    }
}