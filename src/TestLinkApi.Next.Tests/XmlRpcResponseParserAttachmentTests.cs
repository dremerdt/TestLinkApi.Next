using TestLinkApi.Next;
using TestLinkApi.Next.Models;
using Xunit;

namespace TestLinkApi.Next.Tests;

public class XmlRpcResponseParserAttachmentTests
{
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
    public void ParseResponse_ShouldHandleAttachment_WithMinimalFields()
    {
        // Arrange - Attachment with only required fields
        var minimalAttachmentXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>1</name><value><struct>
                        <member><name>id</name><value><string>1</string></value></member>
                        <member><name>name</name><value><string>minimal.txt</string></value></member>
                        <member><name>file_type</name><value><string>text/plain</string></value></member>
                      </struct></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<Attachment>>(minimalAttachmentXml);

        // Assert
        Assert.NotNull(result);
        var attachments = result.ToList();
        Assert.Single(attachments);
        
        var attachment = attachments[0];
        Assert.Equal(1, attachment.Id);
        Assert.Equal("minimal.txt", attachment.Name);
        Assert.Equal("text/plain", attachment.FileType);
        Assert.Null(attachment.Title); // When field is missing from XML, parser returns null
        Assert.Equal(default(DateTime), attachment.DateAdded); // Should default to DateTime.MinValue
        Assert.Null(attachment.Content); // Should be null when not provided
    }

    [Fact]
    public void ParseResponse_ShouldHandleAttachment_WithAllFields()
    {
        // Arrange - Attachment with all possible fields
        var completeAttachmentXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>42</name><value><struct>
                        <member><name>id</name><value><string>42</string></value></member>
                        <member><name>name</name><value><string>complete-attachment.pdf</string></value></member>
                        <member><name>file_type</name><value><string>application/pdf</string></value></member>
                        <member><name>title</name><value><string>Complete Attachment</string></value></member>
                        <member><name>date_added</name><value><string>2025-07-20 10:30:45</string></value></member>
                        <member><name>content</name><value><string>Q29tcGxldGUgYXR0YWNobWVudCBjb250ZW50</string></value></member>
                      </struct></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<Attachment>>(completeAttachmentXml);

        // Assert
        Assert.NotNull(result);
        var attachments = result.ToList();
        Assert.Single(attachments);
        
        var attachment = attachments[0];
        Assert.Equal(42, attachment.Id);
        Assert.Equal("complete-attachment.pdf", attachment.Name);
        Assert.Equal("application/pdf", attachment.FileType);
        Assert.Equal("Complete Attachment", attachment.Title);
        Assert.Equal(new DateTime(2025, 7, 20, 10, 30, 45), attachment.DateAdded);
        
        // Verify base64 content was decoded
        Assert.NotNull(attachment.Content);
        var expectedContent = "Complete attachment content";
        var actualContent = System.Text.Encoding.UTF8.GetString(attachment.Content);
        Assert.Equal(expectedContent, actualContent);
    }

    [Fact]
    public void ParseResponse_ShouldHandleAttachmentRequestResponse_WithMinimalFields()
    {
        // Arrange - AttachmentRequestResponse with only required fields
        var minimalRequestResponseXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>fk_id</name><value><int>10</int></value></member>
                      <member><name>fk_table</name><value><string>testcases</string></value></member>
                      <member><name>file_name</name><value><string>minimal.txt</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<AttachmentRequestResponse>(minimalRequestResponseXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.ForeignKeyId);
        Assert.Equal("testcases", result.LinkedTableName);
        Assert.Equal("minimal.txt", result.FileName);
        Assert.Null(result.Title); // When field is missing from XML, parser returns null
        Assert.Null(result.Description); // When field is missing from XML, parser returns null
        Assert.Equal(0, result.Size); // Should default to 0
        Assert.Null(result.FileType); // When field is missing from XML, parser returns null
    }

    [Fact]
    public void ParseResponse_ShouldHandleBase64Content_Correctly()
    {
        // Arrange - Test various base64 encoded content
        var testCases = new[]
        {
            new { Content = "SGVsbG8gV29ybGQ=", Expected = "Hello World" }, // Simple text
            new { Content = "VGVzdExpbmsgQVBJ", Expected = "TestLink API" }, // Another text
            new { Content = "", Expected = "" }, // Empty content
        };

        foreach (var testCase in testCases)
        {
            var attachmentXml = $"""
                <?xml version="1.0"?>
                <methodResponse>
                  <params>
                    <param>
                      <value>
                        <struct>
                          <member><name>1</name><value><struct>
                            <member><name>id</name><value><string>1</string></value></member>
                            <member><name>name</name><value><string>test.txt</string></value></member>
                            <member><name>file_type</name><value><string>text/plain</string></value></member>
                            <member><name>content</name><value><string>{testCase.Content}</string></value></member>
                          </struct></value></member>
                        </struct>
                      </value>
                    </param>
                  </params>
                </methodResponse>
                """;

            // Act
            var result = XmlRpcResponseParser.ParseResponse<IEnumerable<Attachment>>(attachmentXml);

            // Assert
            var attachment = result.First();
            if (string.IsNullOrEmpty(testCase.Content))
            {
                Assert.True(attachment.Content == null || attachment.Content.Length == 0);
            }
            else
            {
                Assert.NotNull(attachment.Content);
                var actualContent = System.Text.Encoding.UTF8.GetString(attachment.Content);
                Assert.Equal(testCase.Expected, actualContent);
            }
        }
    }

    #region General Upload Response Tests

    [Fact]
    public void ParseResponse_ShouldParseUploadAttachmentResponse_Success()
    {
        // Arrange - Successful response from uploadAttachment
        var uploadAttachmentXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>fk_id</name><value><int>123</int></value></member>
                      <member><name>fk_table</name><value><string>nodes_hierarchy</string></value></member>
                      <member><name>title</name><value><string>General Attachment</string></value></member>
                      <member><name>description</name><value><string>General attachment uploaded via API</string></value></member>
                      <member><name>file_name</name><value><string>document.pdf</string></value></member>
                      <member><name>file_size</name><value><int>2048</int></value></member>
                      <member><name>file_type</name><value><string>application/pdf</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<AttachmentRequestResponse>(uploadAttachmentXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(123, result.ForeignKeyId);
        Assert.Equal("nodes_hierarchy", result.LinkedTableName);
        Assert.Equal("General Attachment", result.Title);
        Assert.Equal("General attachment uploaded via API", result.Description);
        Assert.Equal("document.pdf", result.FileName);
        Assert.Equal(2048, result.Size);
        Assert.Equal("application/pdf", result.FileType);
    }

    [Fact]
    public void ParseResponse_ShouldParseUploadRequirementSpecAttachmentResponse_Success()
    {
        // Arrange - Successful response from uploadRequirementSpecificationAttachment
        var uploadReqSpecAttachmentXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>fk_id</name><value><int>456</int></value></member>
                      <member><name>fk_table</name><value><string>req_specs</string></value></member>
                      <member><name>title</name><value><string>Requirement Specification Document</string></value></member>
                      <member><name>description</name><value><string>Supporting documentation for requirement specification</string></value></member>
                      <member><name>file_name</name><value><string>req_spec.docx</string></value></member>
                      <member><name>file_size</name><value><int>1024</int></value></member>
                      <member><name>file_type</name><value><string>application/vnd.openxmlformats-officedocument.wordprocessingml.document</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<AttachmentRequestResponse>(uploadReqSpecAttachmentXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(456, result.ForeignKeyId);
        Assert.Equal("req_specs", result.LinkedTableName);
        Assert.Equal("Requirement Specification Document", result.Title);
        Assert.Equal("Supporting documentation for requirement specification", result.Description);
        Assert.Equal("req_spec.docx", result.FileName);
        Assert.Equal(1024, result.Size);
        Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", result.FileType);
    }

    [Fact]
    public void ParseResponse_ShouldParseUploadRequirementAttachmentResponse_Success()
    {
        // Arrange - Successful response from uploadRequirementAttachment
        var uploadReqAttachmentXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>fk_id</name><value><int>789</int></value></member>
                      <member><name>fk_table</name><value><string>requirements</string></value></member>
                      <member><name>title</name><value><string>Requirement Supporting Document</string></value></member>
                      <member><name>description</name><value><string>Additional documentation for this requirement</string></value></member>
                      <member><name>file_name</name><value><string>requirement_details.txt</string></value></member>
                      <member><name>file_size</name><value><int>512</int></value></member>
                      <member><name>file_type</name><value><string>text/plain</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;
        // Act
        var result = XmlRpcResponseParser.ParseResponse<AttachmentRequestResponse>(uploadReqAttachmentXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(789, result.ForeignKeyId);
        Assert.Equal("requirements", result.LinkedTableName);
        Assert.Equal("Requirement Supporting Document", result.Title);
        Assert.Equal("Additional documentation for this requirement", result.Description);
        Assert.Equal("requirement_details.txt", result.FileName);
        Assert.Equal(512, result.Size);
        Assert.Equal("text/plain", result.FileType);
    }

    [Fact]
    public void ParseResponse_ShouldHandleUploadAttachmentResponse_WithLargeFileSize()
    {
        // Arrange - Response with large file size
        var largeFileXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>fk_id</name><value><int>888</int></value></member>
                      <member><name>fk_table</name><value><string>test_executions</string></value></member>
                      <member><name>title</name><value><string>Large Test Data File</string></value></member>
                      <member><name>description</name><value><string>Large dataset for performance testing</string></value></member>
                      <member><name>file_name</name><value><string>large_dataset.zip</string></value></member>
                      <member><name>file_size</name><value><int>104857600</int></value></member>
                      <member><name>file_type</name><value><string>application/zip</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;
        // Act
        var result = XmlRpcResponseParser.ParseResponse<AttachmentRequestResponse>(largeFileXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(888, result.ForeignKeyId);
        Assert.Equal("test_executions", result.LinkedTableName);
        Assert.Equal("Large Test Data File", result.Title);
        Assert.Equal("Large dataset for performance testing", result.Description);
        Assert.Equal("large_dataset.zip", result.FileName);
        Assert.Equal(104857600, result.Size); // 100MB
        Assert.Equal("application/zip", result.FileType);
    }

    [Fact]
    public void ParseResponse_ShouldHandleUploadAttachmentResponse_WithSpecialCharacters()
    {
        // Arrange - Response with special characters in file names and descriptions (properly escaped for XML)
        var specialCharsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>fk_id</name><value><int>777</int></value></member>
                      <member><name>fk_table</name><value><string>nodes_hierarchy</string></value></member>
                      <member><name>title</name><value><string>Test-File_With&amp;Special#Characters</string></value></member>
                      <member><name>description</name><value><string>File with éñ¡øñál çhäråçtérs and symbols: @#$%^&amp;*()</string></value></member>
                      <member><name>file_name</name><value><string>test-file_with@special#chars.txt</string></value></member>
                      <member><name>file_size</name><value><int>256</int></value></member>
                      <member><name>file_type</name><value><string>text/plain; charset=utf-8</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;
        
        // Act
        var result = XmlRpcResponseParser.ParseResponse<AttachmentRequestResponse>(specialCharsXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(777, result.ForeignKeyId);
        Assert.Equal("nodes_hierarchy", result.LinkedTableName);
        Assert.Equal("Test-File_With&Special#Characters", result.Title);
        Assert.Equal("File with éñ¡øñál çhäråçtérs and symbols: @#$%^&*()", result.Description);
        Assert.Equal("test-file_with@special#chars.txt", result.FileName);
        Assert.Equal(256, result.Size);
        Assert.Equal("text/plain; charset=utf-8", result.FileType);
    }

    [Fact]
    public void ParseResponse_ShouldHandleUploadAttachment_ZeroByteFile()
    {
        // Arrange - Response for zero-byte file upload
        var zeroByteFileXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>fk_id</name><value><int>555</int></value></member>
                      <member><name>fk_table</name><value><string>nodes_hierarchy</string></value></member>
                      <member><name>title</name><value><string>Empty File</string></value></member>
                      <member><name>description</name><value><string>Zero-byte file for testing</string></value></member>
                      <member><name>file_name</name><value><string>empty.txt</string></value></member>
                      <member><name>file_size</name><value><int>0</int></value></member>
                      <member><name>file_type</name><value><string>text/plain</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;
        // Act
        var result = XmlRpcResponseParser.ParseResponse<AttachmentRequestResponse>(zeroByteFileXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(555, result.ForeignKeyId);
        Assert.Equal("nodes_hierarchy", result.LinkedTableName);
        Assert.Equal("Empty File", result.Title);
        Assert.Equal("Zero-byte file for testing", result.Description);
        Assert.Equal("empty.txt", result.FileName);
        Assert.Equal(0, result.Size);
        Assert.Equal("text/plain", result.FileType);
    }

    #endregion
}