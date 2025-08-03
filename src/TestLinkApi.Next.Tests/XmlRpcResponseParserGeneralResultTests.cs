using TestLinkApi.Next;
using TestLinkApi.Next.Models;
using Xunit;

namespace TestLinkApi.Next.Tests;

public class XmlRpcResponseParserGeneralResultTests
{
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
    public void ParseResponse_ShouldParseGeneralResult_WithAdditionalInfo()
    {
        // Arrange - GeneralResult with additional info struct
        var generalResultWithInfoXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>operation</name><value><string>createTestCase</string></value></member>
                        <member><name>status</name><value><boolean>1</boolean></value></member>
                        <member><name>id</name><value><string>100</string></value></member>
                        <member><name>message</name><value><string>Test case created successfully</string></value></member>
                        <member><name>additionalInfo</name><value><struct>
                          <member><name>id</name><value><string>100</string></value></member>
                          <member><name>external_id</name><value><string>5</string></value></member>
                          <member><name>status_ok</name><value><int>1</int></value></member>
                          <member><name>msg</name><value><string>ok</string></value></member>
                          <member><name>new_name</name><value><string></string></value></member>
                          <member><name>version_number</name><value><int>1</int></value></member>
                          <member><name>has_duplicate</name><value><boolean>0</boolean></value></member>
                        </struct></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(generalResultWithInfoXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("createTestCase", result.Operation);
        Assert.Equal("Test case created successfully", result.Message);
        Assert.True(result.Status);
        Assert.Equal(100, result.Id);
        
        // Verify additional info
        Assert.NotNull(result.AdditionalInfo);
        Assert.Equal(100, result.AdditionalInfo.Id);
        Assert.Equal(5, result.AdditionalInfo.ExternalId);
        Assert.Equal(1, result.AdditionalInfo.VersionNumber);
        Assert.Equal("ok", result.AdditionalInfo.Message);
        Assert.False(result.AdditionalInfo.HasDuplicate);
    }

    [Fact]
    public void ParseResponse_ShouldHandleGeneralResult_WithMinimalFields()
    {
        // Arrange - GeneralResult with only required fields
        var minimalGeneralResultXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>operation</name><value><string>createTestSuite</string></value></member>
                        <member><name>status</name><value><boolean>1</boolean></value></member>
                        <member><name>id</name><value><string>50</string></value></member>
                        <member><name>message</name><value><string>Created</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(minimalGeneralResultXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("createTestSuite", result.Operation);
        Assert.Equal("Created", result.Message);
        Assert.True(result.Status);
        Assert.Equal(50, result.Id);
        Assert.Null(result.AdditionalInfo); // Should be null when not provided
    }

    [Fact]
    public void ParseResponse_ShouldHandleGeneralResult_WithFailureStatus()
    {
        // Arrange - GeneralResult with failure status
        var failureGeneralResultXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>operation</name><value><string>createTestPlan</string></value></member>
                        <member><name>status</name><value><boolean>0</boolean></value></member>
                        <member><name>id</name><value><string>0</string></value></member>
                        <member><name>message</name><value><string>Creation failed: Duplicate name</string></value></member>
                        <member><name>additionalInfo</name><value><struct>
                          <member><name>msg</name><value><string>error</string></value></member>
                          <member><name>has_duplicate</name><value><boolean>1</boolean></value></member>
                        </struct></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(failureGeneralResultXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("createTestPlan", result.Operation);
        Assert.Equal("Creation failed: Duplicate name", result.Message);
        Assert.False(result.Status);
        Assert.Equal(0, result.Id);
        
        // Verify additional info for failure case
        Assert.NotNull(result.AdditionalInfo);
        Assert.Equal("error", result.AdditionalInfo.Message);
        Assert.True(result.AdditionalInfo.HasDuplicate);
    }

    [Fact]
    public void ParseResponse_ShouldHandleGeneralResult_FromSingleStruct()
    {
        // Arrange - GeneralResult returned as single struct (not in array)
        var singleStructXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>operation</name><value><string>updateTestCase</string></value></member>
                      <member><name>status</name><value><boolean>1</boolean></value></member>
                      <member><name>id</name><value><string>75</string></value></member>
                      <member><name>message</name><value><string>Update successful</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(singleStructXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("updateTestCase", result.Operation);
        Assert.Equal("Update successful", result.Message);
        Assert.True(result.Status);
        Assert.Equal(75, result.Id);
        Assert.Null(result.AdditionalInfo);
    }

    [Fact]
    public void ParseResponse_ShouldHandleGeneralResult_WithIntegerStatus()
    {
        // Arrange - GeneralResult with integer status instead of boolean
        var integerStatusXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>operation</name><value><string>deleteTestCase</string></value></member>
                        <member><name>status</name><value><int>1</int></value></member>
                        <member><name>id</name><value><string>25</string></value></member>
                        <member><name>message</name><value><string>Deleted successfully</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(integerStatusXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("deleteTestCase", result.Operation);
        Assert.Equal("Deleted successfully", result.Message);
        Assert.True(result.Status); // Should correctly parse integer 1 as true
        Assert.Equal(25, result.Id);
    }

    [Fact]
    public void ParseResponse_ShouldHandleAdditionalInfo_WithAllFields()
    {
        // Arrange - AdditionalInfo with all possible fields
        var completeAdditionalInfoXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>operation</name><value><string>createTestCase</string></value></member>
                        <member><name>status</name><value><boolean>1</boolean></value></member>
                        <member><name>id</name><value><string>200</string></value></member>
                        <member><name>message</name><value><string>Complete test case creation</string></value></member>
                        <member><name>additionalInfo</name><value><struct>
                          <member><name>id</name><value><string>200</string></value></member>
                          <member><name>external_id</name><value><string>10</string></value></member>
                          <member><name>status_ok</name><value><boolean>1</boolean></value></member>
                          <member><name>msg</name><value><string>complete</string></value></member>
                          <member><name>new_name</name><value><string>Updated Test Case Name</string></value></member>
                          <member><name>version_number</name><value><int>2</int></value></member>
                          <member><name>has_duplicate</name><value><boolean>0</boolean></value></member>
                        </struct></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(completeAdditionalInfoXml);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AdditionalInfo);
        
        var info = result.AdditionalInfo;
        Assert.Equal(200, info.Id);
        Assert.Equal(10, info.ExternalId);
        Assert.True(info.StatusOk);
        Assert.Equal("complete", info.Message);
        Assert.Equal("Updated Test Case Name", info.NewName);
        Assert.Equal(2, info.VersionNumber);
        Assert.False(info.HasDuplicate);
    }

    #region ReportTestCaseResult Response Tests

    [Fact]
    public void ParseResponse_ShouldParseReportTestCaseResultResponse_Success()
    {
        // Arrange - Typical successful response from reportTCResult
        var reportResultXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><n>id</n><value><int>123</int></value></member>
                        <member><n>operation</n><value><string>reportTCResult</string></value></member>
                        <member><n>status</n><value><boolean>1</boolean></value></member>
                        <member><n>message</n><value><string>Test case execution recorded successfully</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(reportResultXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(123, result.Id);
        Assert.Equal("reportTCResult", result.Operation);
        Assert.True(result.Status);
        Assert.Equal("Test case execution recorded successfully", result.Message);
    }

    [Fact]
    public void ParseResponse_ShouldParseReportTestCaseResultResponse_WithAdditionalInfo()
    {
        // Arrange - Response with additional execution information
        var reportResultXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><n>id</n><value><int>456</int></value></member>
                      <member><n>operation</n><value><string>reportTCResult</string></value></member>
                      <member><n>status</n><value><boolean>1</boolean></value></member>
                      <member><n>message</n><value><string>Execution created</string></value></member>
                      <member><n>additionalInfo</n><value><struct>
                        <member><n>execution_id</n><value><int>789</int></value></member>
                        <member><n>status_ok</n><value><boolean>1</boolean></value></member>
                        <member><n>message</n><value><string>Test case passed successfully</string></value></member>
                      </struct></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(reportResultXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(456, result.Id);
        Assert.Equal("reportTCResult", result.Operation);
        Assert.True(result.Status);
        Assert.Equal("Execution created", result.Message);
        Assert.NotNull(result.AdditionalInfo);
        Assert.True(result.AdditionalInfo.StatusOk);
        Assert.Equal("Test case passed successfully", result.AdditionalInfo.Message);
    }

    [Fact]
    public void ParseResponse_ShouldParseReportTestCaseResultResponse_WithBlockedStatus()
    {
        // Arrange - Response for blocked test case result
        var reportBlockedXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><n>id</n><value><int>999</int></value></member>
                        <member><n>operation</n><value><string>reportTCResult</string></value></member>
                        <member><n>status</n><value><boolean>1</boolean></value></member>
                        <member><n>message</n><value><string>Test case blocked due to environment issues</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(reportBlockedXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(999, result.Id);
        Assert.Equal("reportTCResult", result.Operation);
        Assert.True(result.Status);
        Assert.Equal("Test case blocked due to environment issues", result.Message);
    }

    [Fact]
    public void ParseResponse_ShouldParseReportTestCaseResultResponse_WithEmptyNotes()
    {
        // Arrange - Response with minimal information
        var reportMinimalXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><n>id</n><value><int>111</int></value></member>
                        <member><n>operation</n><value><string>reportTCResult</string></value></member>
                        <member><n>status</n><value><boolean>1</boolean></value></member>
                        <member><n>message</n><value><string></string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(reportMinimalXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(111, result.Id);
        Assert.Equal("reportTCResult", result.Operation);
        Assert.True(result.Status);
        Assert.Equal(string.Empty, result.Message);
    }

    [Fact]
    public void ParseResponse_ShouldParseAssignRequirementsResponse_Success()
    {
        // Arrange - Successful response from assignRequirements
        var assignRequirementsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><n>id</n><value><int>0</int></value></member>
                      <member><n>operation</n><value><string>assignRequirements</string></value></member>
                      <member><n>status</n><value><boolean>1</boolean></value></member>
                      <member><n>message</n><value><string>Requirements assigned successfully</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(assignRequirementsXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Equal("assignRequirements", result.Operation);
        Assert.True(result.Status);
        Assert.Equal("Requirements assigned successfully", result.Message);
    }

    [Fact]
    public void ParseResponse_ShouldParseAssignRequirementsResponse_WithMultipleRequirements()
    {
        // Arrange - Response when assigning multiple requirements
        var assignMultipleRequirementsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><n>id</n><value><int>0</int></value></member>
                      <member><n>operation</n><value><string>assignRequirements</string></value></member>
                      <member><n>status</n><value><boolean>1</boolean></value></member>
                      <member><n>message</n><value><string>3 requirements assigned to test case</string></value></member>
                      <member><n>additionalInfo</n><value><struct>
                        <member><n>assigned_count</n><value><int>3</int></value></member>
                        <member><n>status_ok</n><value><boolean>1</boolean></value></member>
                      </struct></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(assignMultipleRequirementsXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Equal("assignRequirements", result.Operation);
        Assert.True(result.Status);
        Assert.Equal("3 requirements assigned to test case", result.Message);
        Assert.NotNull(result.AdditionalInfo);
        Assert.True(result.AdditionalInfo.StatusOk);
    }

    [Fact]
    public void ParseResponse_ShouldParseAssignRequirementsResponse_WithEmptyRequirementsList()
    {
        // Arrange - Response when no requirements are assigned (edge case)
        var emptyAssignXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><n>id</n><value><int>0</int></value></member>
                      <member><n>operation</n><value><string>assignRequirements</string></value></member>
                      <member><n>status</n><value><boolean>1</boolean></value></member>
                      <member><n>message</n><value><string>No requirements to assign</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(emptyAssignXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Equal("assignRequirements", result.Operation);
        Assert.True(result.Status);
        Assert.Equal("No requirements to assign", result.Message);
    }

    [Fact]
    public void ParseResponse_ShouldParseAssignRequirements_PartialSuccess()
    {
        // Arrange - Response when some requirements were assigned but others failed
        var partialSuccessXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><n>id</n><value><int>0</int></value></member>
                      <member><n>operation</n><value><string>assignRequirements</string></value></member>
                      <member><n>status</n><value><boolean>1</boolean></value></member>
                      <member><n>message</n><value><string>2 of 3 requirements assigned successfully</string></value></member>
                      <member><n>additionalInfo</n><value><struct>
                        <member><n>assigned_count</n><value><int>2</int></value></member>
                        <member><n>failed_count</n><value><int>1</int></value></member>
                        <member><n>status_ok</n><value><boolean>1</boolean></value></member>
                        <member><n>message</n><value><string>Some requirements could not be assigned due to permissions</string></value></member>
                      </struct></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<GeneralResult>(partialSuccessXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Equal("assignRequirements", result.Operation);
        Assert.True(result.Status);
        Assert.Equal("2 of 3 requirements assigned successfully", result.Message);
        Assert.NotNull(result.AdditionalInfo);
        Assert.True(result.AdditionalInfo.StatusOk);
        Assert.Equal("Some requirements could not be assigned due to permissions", result.AdditionalInfo.Message);
    }

    #endregion
}