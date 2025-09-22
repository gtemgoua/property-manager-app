namespace PropertyManager.Api.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, byte[]? attachment = null, string? attachmentFileName = null, string? attachmentContentType = null, CancellationToken cancellationToken = default);
}
