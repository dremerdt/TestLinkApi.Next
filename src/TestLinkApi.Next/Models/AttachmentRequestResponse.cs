namespace TestLinkApi.Next.Models;

public class AttachmentRequestResponse
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int ForeignKeyId { get; set; }
    public string LinkedTableName { get; set; } = string.Empty;
    public int Size { get; set; }
}