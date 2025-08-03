namespace TestLinkApi.Next.Contracts;

using TestLinkApi.Next.Models;

/// <summary>
/// Interface for TestLink attachment and requirement operations
/// </summary>
public interface IAttachmentOperations
{
    /// <summary>
    /// Upload a general attachment (can be attached to various TestLink items)
    /// </summary>
    /// <param name="foreignKeyId">ID of the item to attach to</param>
    /// <param name="foreignKeyTable">Table name of the item type</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="fileType">MIME type of the file</param>
    /// <param name="content">File content as byte array</param>
    /// <param name="title">Optional title for the attachment</param>
    /// <param name="description">Optional description for the attachment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the upload operation</returns>
    Task<AttachmentRequestResponse> UploadAttachmentAsync(
        int foreignKeyId,
        string foreignKeyTable,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload an attachment to a requirement specification
    /// </summary>
    /// <param name="requirementSpecificationId">ID of the requirement specification</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="fileType">MIME type of the file</param>
    /// <param name="content">File content as byte array</param>
    /// <param name="title">Optional title for the attachment</param>
    /// <param name="description">Optional description for the attachment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the upload operation</returns>
    Task<AttachmentRequestResponse> UploadRequirementSpecificationAttachmentAsync(
        int requirementSpecificationId,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload an attachment to a requirement
    /// </summary>
    /// <param name="requirementId">ID of the requirement</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="fileType">MIME type of the file</param>
    /// <param name="content">File content as byte array</param>
    /// <param name="title">Optional title for the attachment</param>
    /// <param name="description">Optional description for the attachment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the upload operation</returns>
    Task<AttachmentRequestResponse> UploadRequirementAttachmentAsync(
        int requirementId,
        string fileName,
        string fileType,
        byte[] content,
        string? title = null,
        string? description = null,
        CancellationToken cancellationToken = default);
}