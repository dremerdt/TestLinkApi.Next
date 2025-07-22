namespace TestLinkApi.Next;

/// <summary>
/// Base exception for TestLink API operations
/// </summary>
public class TestLinkApiException : Exception
{
    public TestLinkApiException()
    {
    }

    public TestLinkApiException(string message) : base(message)
    {
    }

    public TestLinkApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
public class TestLinkAuthenticationException : TestLinkApiException
{
    public TestLinkAuthenticationException() : base("Authentication failed. Please check your developer key.")
    {
    }

    public TestLinkAuthenticationException(string message) : base(message)
    {
    }

    public TestLinkAuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class TestLinkNotFoundException : TestLinkApiException
{
    public TestLinkNotFoundException()
    {
    }

    public TestLinkNotFoundException(string message) : base(message)
    {
    }

    public TestLinkNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when there are validation errors in the request
/// </summary>
public class TestLinkValidationException : TestLinkApiException
{
    public List<string> ValidationErrors { get; }

    public TestLinkValidationException(IEnumerable<string> validationErrors) 
        : base($"Validation failed: {string.Join(", ", validationErrors)}")
    {
        ValidationErrors = validationErrors.ToList();
    }

    public TestLinkValidationException(string validationError) 
        : base($"Validation failed: {validationError}")
    {
        ValidationErrors = new List<string> { validationError };
    }
}

/// <summary>
/// Represents an error message returned by the TestLink API
/// </summary>
public record TestLinkErrorMessage
{
    public int Code { get; init; }
    public string Message { get; init; } = string.Empty;
}