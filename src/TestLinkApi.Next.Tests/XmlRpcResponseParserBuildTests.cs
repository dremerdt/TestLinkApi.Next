using TestLinkApi.Next;
using TestLinkApi.Next.Models;
using Xunit;

namespace TestLinkApi.Next.Tests;

public class XmlRpcResponseParserBuildTests
{
    [Fact]
    public void ParseResponse_ShouldParseBuild_WithTestPlanIdMapping()
    {
        // Arrange - Build XML response similar to the one mentioned in the user's issue
        var buildXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><name>id</name><value><string>6</string></value></member>
                      <member><name>testplan_id</name><value><string>483</string></value></member>
                      <member><name>name</name><value><string>Build_4ebad35434fb458a9046552eebe3d7d5</string></value></member>
                      <member><name>notes</name><value><string></string></value></member>
                      <member><name>active</name><value><string>1</string></value></member>
                      <member><name>is_open</name><value><string>1</string></value></member>
                      <member><name>release_date</name><value><string></string></value></member>
                      <member><name>closed_on_date</name><value><string></string></value></member>
                      <member><name>creation_ts</name><value><string>2025-08-03 10:51:26</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<Build>(buildXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.Id);
        Assert.Equal(483, result.TestPlanId); // This should now correctly map from testplan_id
        Assert.Equal("Build_4ebad35434fb458a9046552eebe3d7d5", result.Name);
        Assert.Equal(string.Empty, result.Notes);
        Assert.True(result.Active);
        Assert.True(result.IsOpen);
    }

    [Fact]
    public void ParseResponse_ShouldParseMultipleBuilds_WithTestPlanIdMapping()
    {
        // Arrange - Multiple builds in array
        var multipleBuildsXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <array><data>
                      <value><struct>
                        <member><name>id</name><value><string>1</string></value></member>
                        <member><name>testplan_id</name><value><string>100</string></value></member>
                        <member><name>name</name><value><string>Build One</string></value></member>
                        <member><name>notes</name><value><string>First build</string></value></member>
                        <member><name>active</name><value><string>1</string></value></member>
                        <member><name>is_open</name><value><string>1</string></value></member>
                      </struct></value>
                      <value><struct>
                        <member><name>id</name><value><string>2</string></value></member>
                        <member><name>testplan_id</name><value><string>200</string></value></member>
                        <member><name>name</name><value><string>Build Two</string></value></member>
                        <member><name>notes</name><value><string>Second build</string></value></member>
                        <member><name>active</name><value><string>0</string></value></member>
                        <member><name>is_open</name><value><string>0</string></value></member>
                      </struct></value>
                    </data></array>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<Build>>(multipleBuildsXml);

        // Assert
        Assert.NotNull(result);
        var builds = result.ToList();
        Assert.Equal(2, builds.Count);
        
        // First build
        var build1 = builds[0];
        Assert.Equal(1, build1.Id);
        Assert.Equal(100, build1.TestPlanId); // Should correctly map from testplan_id
        Assert.Equal("Build One", build1.Name);
        Assert.Equal("First build", build1.Notes);
        Assert.True(build1.Active);
        Assert.True(build1.IsOpen);
        
        // Second build
        var build2 = builds[1];
        Assert.Equal(2, build2.Id);
        Assert.Equal(200, build2.TestPlanId); // Should correctly map from testplan_id
        Assert.Equal("Build Two", build2.Name);
        Assert.Equal("Second build", build2.Notes);
        Assert.False(build2.Active);
        Assert.False(build2.IsOpen);
    }

    [Fact]
    public void ParseResponse_ShouldHandleEmptyBuildsList_WhenNoBuildsExist()
    {
        // Arrange - Empty array response when no builds exist
        var emptyBuildsXml = """
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
        var result = XmlRpcResponseParser.ParseResponse<IEnumerable<Build>>(emptyBuildsXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Debug_TestPlanIdMapping_ShouldWork()
    {
        // Arrange - Simplified XML to debug the exact issue
        var buildXml = """
            <?xml version="1.0"?>
            <methodResponse>
              <params>
                <param>
                  <value>
                    <struct>
                      <member><n>id</n><value><string>6</string></value></member>
                      <member><n>testplan_id</n><value><string>483</string></value></member>
                      <member><n>name</n><value><string>Build_Test</string></value></member>
                      <member><n>notes</n><value><string></string></value></member>
                      <member><n>active</n><value><string>1</string></value></member>
                      <member><n>is_open</n><value><string>1</string></value></member>
                    </struct>
                  </value>
                </param>
              </params>
            </methodResponse>
            """;

        // Act
        var result = XmlRpcResponseParser.ParseResponse<Build>(buildXml);

        // Assert - Debug output to see what's actually happening
        Assert.NotNull(result);
        Assert.Equal(6, result.Id);
        Assert.NotEqual(0, result.TestPlanId); // Ensure it's not 0
        Assert.Equal(483, result.TestPlanId); // Should correctly map from testplan_id
        Assert.Equal("Build_Test", result.Name);
        Assert.Equal(string.Empty, result.Notes);
        Assert.True(result.Active);
        Assert.True(result.IsOpen);
    }
}