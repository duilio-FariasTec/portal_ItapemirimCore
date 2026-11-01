namespace Itapemirim.Ani.Infrastructure.Email;

using System.Net;
using System.Net.Mail;
using System.Text;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarAsync(string destinatario, string assunto, string corpoHtml, CancellationToken ct = default)
    {
        var host = _configuration["Email:Smtp:Host"];
        var port = int.TryParse(_configuration["Email:Smtp:Port"], out var p) ? p : 587;
        var enableSsl = !bool.TryParse(_configuration["Email:Smtp:EnableSsl"], out var ssl) || ssl;
        var username = _configuration["Email:Smtp:Username"];
        var password = _configuration["Email:Smtp:Password"];
        var fromAddress = _configuration["Email:Smtp:FromAddress"] ?? "no-reply@itapemirim.com.br";
        var fromName = _configuration["Email:Smtp:FromName"] ?? "Portal Corporativo ANI — Grupo Itapemirim";

        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning(
                "SmtpEmailService acionado, mas 'Email:Smtp:Host' não está configurado. E-mail para '{Destinatario}' não pôde ser despachado via rede.",
                destinatario);
            throw new InvalidOperationException("O servidor SMTP ('Email:Smtp:Host') não está configurado no ambiente.");
        }

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName, Encoding.UTF8),
            Subject = assunto,
            SubjectEncoding = Encoding.UTF8,
            Body = corpoHtml,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };

        mailMessage.To.Add(new MailAddress(destinatario));

        _logger.LogInformation("Enviando e-mail via SMTP ({Host}:{Port}) para '{Destinatario}'...", host, port, destinatario);
        await client.SendMailAsync(mailMessage, ct);
        _logger.LogInformation("E-mail enviado via SMTP com sucesso para '{Destinatario}'.", destinatario);
    }
}

