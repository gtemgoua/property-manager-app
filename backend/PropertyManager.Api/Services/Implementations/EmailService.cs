using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PropertyManager.Api.Options;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Services.Implementations;

public class EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = options.Value;

    public async Task SendEmailAsync(string to, string subject, string htmlBody, byte[]? attachment = null, string? attachmentFileName = null, string? attachmentContentType = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            logger.LogWarning("Email host not configured. Skipping email send to {Recipient}", to);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        if (attachment is not null && attachmentFileName is not null)
        {
            builder.Attachments.Add(attachmentFileName, attachment, ContentType.Parse(attachmentContentType ?? "application/octet-stream"));
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, cancellationToken);

        if (!string.IsNullOrEmpty(_settings.Username))
        {
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("Email sent to {Recipient}", to);
    }
}
