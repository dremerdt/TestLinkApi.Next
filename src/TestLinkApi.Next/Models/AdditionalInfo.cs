namespace TestLinkApi.Next.Models;

/// <summary>
/// Additional Info returned by the API for operations like test case creation
/// </summary>
public class AdditionalInfo
{
    public int ExternalId { get; init; }
    public bool? HasDuplicate { get; init; }
    public int Id { get; init; }
    public string Message { get; init; } = string.Empty;
    public string NewName { get; init; } = string.Empty;
    public bool StatusOk { get; init; }
    public int VersionNumber { get; init; }
}