namespace TestLinkApi.Next.Models;

public class GeneralResult
{
    public AdditionalInfo? AdditionalInfo { get; set; }
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public bool Status { get; set; }
}