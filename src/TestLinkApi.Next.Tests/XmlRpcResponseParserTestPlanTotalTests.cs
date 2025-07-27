using TestLinkApi.Next;
using TestLinkApi.Next.Models;
using Xunit;

namespace TestLinkApi.Next.Tests;

public class XmlRpcResponseParserTestPlanTotalTests
{
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
    public void ParseResponse_ShouldParseMultipleTestPlanTotals_WithDifferentTypes()
    {
        // Arrange - Multiple TestPlanTotal entries with different types
        var multipleTestPlanTotalsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>type</name><value><string>platform</string></value></member>
                        <member><name>name</name><value><string>Windows</string></value></member>
                        <member><name>total_tc</name><value><int>15</int></value></member>
                        <member><name>details</name><value><struct>
                          <member><name>passed</name><value><int>10</int></value></member>
                          <member><name>failed</name><value><int>3</int></value></member>
                          <member><name>blocked</name><value><int>2</int></value></member>
                        </struct></value></member>
                      </struct></value>
                      <value><struct>
                        <member><name>type</name><value><string>platform</string></value></member>
                        <member><name>name</name><value><string>Linux</string></value></member>
                        <member><name>total_tc</name><value><int>12</int></value></member>
                        <member><name>details</name><value><struct>
                          <member><name>passed</name><value><int>8</int></value></member>
                          <member><name>failed</name><value><int>2</int></value></member>
                          <member><name>blocked</name><value><int>1</int></value></member>
                          <member><name>not_run</name><value><int>1</int></value></member>
                        </struct></value></member>
                      </struct></value>
                      <value><struct>
                        <member><name>type</name><value><string>summary</string></value></member>
                        <member><name>total_tc</name><value><int>27</int></value></member>
                        <member><name>details</name><value><struct>
                          <member><name>passed</name><value><int>18</int></value></member>
                          <member><name>failed</name><value><int>5</int></value></member>
                          <member><name>blocked</name><value><int>3</int></value></member>
                          <member><name>not_run</name><value><int>1</int></value></member>
                        </struct></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlanTotal>>(multipleTestPlanTotalsXml);

        // Assert
        Assert.NotNull(result);
        var totals = result.ToList();
        Assert.Equal(3, totals.Count);

        // Windows platform
        var windowsTotal = totals.FirstOrDefault(t => t.Name == "Windows");
        Assert.NotNull(windowsTotal);
        Assert.Equal("platform", windowsTotal.Type);
        Assert.Equal(15, windowsTotal.TotalTestCases);
        Assert.Equal(10, windowsTotal.Details["passed"]);
        Assert.Equal(3, windowsTotal.Details["failed"]);
        Assert.Equal(2, windowsTotal.Details["blocked"]);

        // Linux platform
        var linuxTotal = totals.FirstOrDefault(t => t.Name == "Linux");
        Assert.NotNull(linuxTotal);
        Assert.Equal("platform", linuxTotal.Type);
        Assert.Equal(12, linuxTotal.TotalTestCases);
        Assert.Equal(8, linuxTotal.Details["passed"]);
        Assert.Equal(2, linuxTotal.Details["failed"]);
        Assert.Equal(1, linuxTotal.Details["blocked"]);
        Assert.Equal(1, linuxTotal.Details["not_run"]);

        // Summary
        var summaryTotal = totals.FirstOrDefault(t => t.Type == "summary");
        Assert.NotNull(summaryTotal);
        Assert.Equal(27, summaryTotal.TotalTestCases);
        Assert.Equal(18, summaryTotal.Details["passed"]);
        Assert.Equal(5, summaryTotal.Details["failed"]);
        Assert.Equal(3, summaryTotal.Details["blocked"]);
        Assert.Equal(1, summaryTotal.Details["not_run"]);
    }

    [Fact]
    public void ParseResponse_ShouldHandleTestPlanTotal_WithMinimalFields()
    {
        // Arrange - TestPlanTotal with only required fields
        var minimalTestPlanTotalXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>total_tc</name><value><int>5</int></value></member>
                        <member><name>details</name><value><struct>
                          <member><name>passed</name><value><int>5</int></value></member>
                        </struct></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<TestPlanTotal>>(minimalTestPlanTotalXml);

        // Assert
        Assert.NotNull(result);
        var totals = result.ToList();
        Assert.Single(totals);

        var total = totals[0];
        Assert.Null(total.Type); // When field is missing from XML, parser returns null
        Assert.Null(total.Name); // When field is missing from XML, parser returns null
        Assert.Equal(5, total.TotalTestCases);
        Assert.NotNull(total.Details);
        Assert.Single(total.Details);
        Assert.Equal(5, total.Details["passed"]);
    }
}