namespace TestLinkApi.Next.Models;

/// <summary>
/// Represents an attachment
/// </summary>
public record Attachment
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string FileType { get; init; } = string.Empty;
    public DateTime DateAdded { get; init; }
    public byte[]? Content { get; init; }
}