namespace TestLinkApi.Next.Models;

/// <summary>
/// Generic API result wrapper
/// </summary>
public record ApiResult
{
    public bool Status { get; init; }
    public int Id { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Operation { get; init; } = string.Empty;
    public AdditionalInfo? AdditionalInfo { get; init; }
    public bool Overwrite { get; init; }
}