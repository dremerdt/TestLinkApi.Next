using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;

namespace TestLinkApi.Next.Tests;

/// <summary>
/// Base class for TestLink API tests
/// </summary>
public abstract class TestLinkTestBase
{
    protected readonly TestLinkSettings Settings;
    protected readonly TestLinkClient Client;

    protected TestLinkTestBase()
    {
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        Settings = new TestLinkSettings();
        configuration.GetSection("TestLinkSettings").Bind(Settings);
        if (string.IsNullOrEmpty(Settings.ApiKey) || string.IsNullOrEmpty(Settings.BaseUrl))
        {
            throw new InvalidOperationException("TestLink settings are not properly configured.");
        }
        // Create TestLink client
        Client = TestLinkClientBuilder.Create()
            .WithBaseUrl(Settings.BaseUrl)
            .WithApiKey(Settings.ApiKey)
            .Build();
    }
}