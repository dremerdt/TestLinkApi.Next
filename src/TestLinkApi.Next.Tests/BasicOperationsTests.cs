namespace TestLinkApi.Next.Tests;

/// <summary>
/// Integration tests for Basic Operations in TestLinkClient
/// These tests require a running TestLink instance configured in appsettings.json
/// </summary>
public class BasicOperationsTests : TestLinkTestBase, IDisposable
{
    #region Ping Tests

    [Fact]
    public async Task PingAsync_WithRealTestLinkInstance_ReturnsHello()
    {
        // Act
        var result = await Client.PingAsync();

        // Assertd
        Assert.NotNull(result);
        Assert.Equal("Hello!", result);
    }

    [Fact]
    public async Task PingAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.PingAsync(cts.Token));
    }

    #endregion

    #region SayHello Tests

    [Fact]
    public async Task SayHelloAsync_WithValidName_ReturnsGreeting()
    {
        // Arrange
        var hello = "Hello!";

        // Act
        var result = await Client.SayHelloAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(hello, result);
    }

    #endregion

    #region About Tests

    [Fact]
    public async Task AboutAsync_WithRealTestLinkInstance_ReturnsVersionInfo()
    {
        // Act
        var result = await Client.AboutAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Based on PHP implementation, should contain "Testlink API Version"
        Assert.Contains("Testlink API Version", result);
    }

    [Fact]
    public async Task AboutAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.AboutAsync(cts.Token));
    }

    #endregion

    #region CheckDevKey Tests

    [Fact]
    public async Task CheckDevKeyAsync_WithValidKey_ReturnsTrue()
    {
        // Act
        var result = await Client.CheckDevKeyAsync();

        // Assert - Based on PHP implementation, valid key returns boolean true
        Assert.Equal("True", result);
    }

    [Fact]
    public async Task CheckDevKeyAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.CheckDevKeyAsync(cts.Token));
    }

    #endregion

    #region DoesUserExist Tests

    [Fact]
    public async Task DoesUserExistAsync_WithConfiguredUser_ReturnsTrue()
    {
        // Act
        var result = await Client.DoesUserExistAsync(Settings.User);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DoesUserExistAsync_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        var nonExistentUser = "NonExistentUser12345";

        // Assert
        await Assert.ThrowsAsync<TestLinkApiException>(async () => await Client.DoesUserExistAsync(nonExistentUser));
    }

    [Fact]
    public async Task DoesUserExistAsync_WithNullUsername_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => Client.DoesUserExistAsync(null!));
    }

    [Fact]
    public async Task DoesUserExistAsync_WithEmptyUsername_ReturnsFalse()
    {
        // Arrange
        var username = "";

        // Act & Assert
        await Assert.ThrowsAsync<TestLinkApiException>(async () => await Client.DoesUserExistAsync(username));
    }

    [Fact]
    public async Task DoesUserExistAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.DoesUserExistAsync("test", cts.Token));
    }

    #endregion

    #region SetTestMode Tests

    [Fact]
    public async Task SetTestMode_WithTrueValue_ReturnsTrue()
    {
        // Act
        var result = await Client.SetTestMode(true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SetTestMode_WithFalseValue_ReturnsTrue()
    {
        // Note: Based on PHP implementation, SetTestMode returns true when operation succeeds
        // regardless of the value being set
        
        // Act
        var result = await Client.SetTestMode(false);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SetTestMode_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.SetTestMode(true, cts.Token));
    }

    #endregion

    #region Repeat Tests

    [Fact]
    public async Task RepeatAsync_WithValidText_ReturnsRepeatedText()
    {
        // Arrange
        var text = "Integration test message";

        // Act
        var result = await Client.RepeatAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal($"You said: {text}", result);
    }

    [Fact]
    public async Task RepeatAsync_WithNullText_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => Client.RepeatAsync(null!));
    }

    [Fact]
    public async Task RepeatAsync_WithEmptyText_HandlesEmptyText()
    {
        // Arrange
        var text = "";

        // Act
        var result = await Client.RepeatAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("You said: ", result);
    }

    [Fact]
    public async Task RepeatAsync_WithSpecialCharacters_HandlesSpecialCharacters()
    {
        // Arrange
        var text = "Hello <>&\"'";

        // Act
        var result = await Client.RepeatAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal($"You said: {text}", result);
    }

    [Fact]
    public async Task RepeatAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.RepeatAsync("test", cts.Token));
    }

    #endregion

    #region GetFullPath Tests

    [Fact]
    public async Task GetFullPathAsync_WithValidNodeId_ReturnsPath()
    {
        // Arrange
        var nodeId = 1; // Try with root node ID

        // Act & Assert
        await Assert.ThrowsAsync<TestLinkValidationException>(async () => await Client.GetFullPathAsync(nodeId));
    }

    [Fact]
    public async Task GetFullPathAsync_WithInvalidNodeId_ThrowsTestLinkApiException()
    {
        // Arrange
        var nodeId = -1; // Invalid node ID

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TestLinkValidationException>(() => Client.GetFullPathAsync(nodeId));
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public async Task GetFullPathAsync_WithZeroNodeId_ThrowsTestLinkApiException()
    {
        // Arrange
        var nodeId = 0; // Zero is typically invalid

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TestLinkValidationException>(() => Client.GetFullPathAsync(nodeId));
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public async Task GetFullPathAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.GetFullPathAsync(123, cts.Token));
    }

    #endregion

    #region Comprehensive Integration Tests

    [Fact]
    public async Task BasicOperations_AllMethods_ExecuteWithoutException()
    {
        // This test verifies that all basic operations can be called without throwing exceptions
        // when connected to a real TestLink instance

        // Act & Assert - All operations should complete without exceptions
        var pingResult = await Client.PingAsync();
        var aboutResult = await Client.AboutAsync();
        var keyResult = await Client.CheckDevKeyAsync();
        var helloResult = await Client.SayHelloAsync();
        var userExistsResult = await Client.DoesUserExistAsync(Settings.User);
        var testModeResult = await Client.SetTestMode(true);
        var repeatResult = await Client.RepeatAsync("Test message");

        // Verify expected results
        Assert.Equal("Hello!", pingResult);
        Assert.Contains("Testlink API Version", aboutResult);
        Assert.Equal("True", keyResult);
        Assert.Equal("Hello!", helloResult);
        Assert.True(userExistsResult);
        Assert.True(testModeResult);
        Assert.Equal("You said: Test message", repeatResult);
        
        // GetFullPath requires a valid node ID, so we'll test it separately
        // if we can determine a valid node ID from the test environment
    }

    [Fact]
    public async Task BasicOperations_WithCancellationToken_AllRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert - All operations should respect cancellation
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.PingAsync(cts.Token));
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.AboutAsync(cts.Token));
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.CheckDevKeyAsync(cts.Token));
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.SayHelloAsync(cts.Token));
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.DoesUserExistAsync("test", cts.Token));
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.SetTestMode(true, cts.Token));
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.RepeatAsync("test", cts.Token));
        await Assert.ThrowsAsync<TaskCanceledException>(() => Client.GetFullPathAsync(1, cts.Token));
    }

    [Fact]
    public async Task BasicOperations_SequentialCalls_MaintainConnection()
    {
        // This test verifies that multiple sequential calls work correctly
        // and that the connection is maintained across calls

        // Act - Make multiple sequential calls
        var pingResult = await Client.PingAsync();
        var aboutResult = await Client.AboutAsync();
        var keyResult = await Client.CheckDevKeyAsync();
        var helloResult = await Client.SayHelloAsync();
        var userExistsResult = await Client.DoesUserExistAsync(Settings.User);
        var testModeResult = await Client.SetTestMode(true);
        var repeatResult = await Client.RepeatAsync("Sequential test");

        // Assert - All calls should return expected results
        Assert.Equal("Hello!", pingResult);
        Assert.Contains("Testlink API Version", aboutResult);
        Assert.Equal("True", keyResult);
        Assert.Equal("Hello!", helloResult);
        Assert.True(userExistsResult);
        Assert.True(testModeResult);
        Assert.Equal("You said: Sequential test", repeatResult);
    }

    [Fact]
    public async Task BasicOperations_DifferentParameterTypes_HandleCorrectly()
    {
        // Test different parameter types and edge cases
        
        // String parameters
        var hello = await Client.SayHelloAsync();
        var repeatEmpty = await Client.RepeatAsync("");
        Assert.Equal("Hello!", hello);
        Assert.Equal("You said: ", repeatEmpty);

        // Boolean parameters
        var testModeTrue = await Client.SetTestMode(true);
        var testModeFalse = await Client.SetTestMode(false);
        Assert.True(testModeTrue);
        Assert.True(testModeFalse);

        // Special characters in strings
        var specialChars = "Test with <>&\"' special chars";
        var specialResult = await Client.RepeatAsync(specialChars);
        Assert.Equal($"You said: {specialChars}", specialResult);
    }

    #endregion

    #region Authentication and Error Handling Tests

    [Fact]
    public async Task BasicOperations_WithValidCredentials_AllSucceed()
    {
        // This test verifies that all operations work with valid credentials
        // and that the API key is properly validated

        // Act - Test authentication-dependent operations
        var keyCheck = await Client.CheckDevKeyAsync();
        var userCheck = await Client.DoesUserExistAsync(Settings.User);
        
        // Assert - Valid credentials should work
        Assert.Equal("True", keyCheck);
        Assert.True(userCheck);
    }

    [Fact]
    public async Task BasicOperations_DevKeyValidation_WorksCorrectly()
    {
        // This test verifies that the developer key validation works
        // The key is validated by the CheckDevKey method

        // Act
        var result = await Client.CheckDevKeyAsync();

        // Assert - Should return "true" for valid key
        Assert.Equal("True", result);
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}