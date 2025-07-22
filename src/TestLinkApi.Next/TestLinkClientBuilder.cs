namespace TestLinkApi.Next;

/// <summary>
/// Fluent builder for creating TestLink API client instances
/// </summary>
public class TestLinkClientBuilder
{
    private string? _apiKey;
    private string? _baseUrl;
    private HttpClient? _httpClient;
    private TimeSpan _timeout = TimeSpan.FromMinutes(5);
    private bool _validateCertificate = true;

    /// <summary>
    /// Sets the API key (developer key)
    /// </summary>
    public TestLinkClientBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey;
        return this;
    }

    /// <summary>
    /// Sets the base URL for the TestLink API
    /// </summary>
    public TestLinkClientBuilder WithBaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl;
        return this;
    }

    /// <summary>
    /// Sets a custom HttpClient instance
    /// </summary>
    public TestLinkClientBuilder WithHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        return this;
    }

    /// <summary>
    /// Sets the timeout for HTTP requests
    /// </summary>
    public TestLinkClientBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <summary>
    /// Configures SSL certificate validation
    /// </summary>
    public TestLinkClientBuilder WithCertificateValidation(bool validate)
    {
        _validateCertificate = validate;
        return this;
    }

    /// <summary>
    /// Builds the TestLink client instance
    /// </summary>
    public TestLinkClient Build()
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new ArgumentException("API key is required");

        if (string.IsNullOrEmpty(_baseUrl))
            throw new ArgumentException("Base URL is required");

        HttpClient httpClient;
        if (_httpClient != null)
        {
            httpClient = _httpClient;
        }
        else
        {
            var handler = new HttpClientHandler();
            if (!_validateCertificate)
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }

            httpClient = new HttpClient(handler)
            {
                Timeout = _timeout
            };
        }

        return new TestLinkClient(_apiKey, _baseUrl, httpClient);
    }

    /// <summary>
    /// Creates a new builder instance
    /// </summary>
    public static TestLinkClientBuilder Create() => new();
}