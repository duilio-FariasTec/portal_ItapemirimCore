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
    private readonly FileEmailService _fileEmailService;

    public SmtpEmailService(
        IConfiguration configuration,
        ILogger<SmtpEmailService> logger,
        FileEmailService fileEmailService)
    {
        _configuration = configuration;
        _logger = logger;
        _fileEmailService = fileEmailService;
    }

    public async Task EnviarAsync(string destinatario, string assunto, string corpoHtml, CancellationToken ct = default)
    {
        var alsoWriteToFile = !bool.TryParse(_configuration["Email:AlsoWriteToFile"], out var writeFile) || writeFile;
        var fallbackToFileOnError = !bool.TryParse(_configuration["Email:FallbackToFileOnError"], out var fallback) || fallback;

        if (alsoWriteToFile)
        {
            await _fileEmailService.EnviarAsync(destinatario, assunto, corpoHtml, ct);
        }

        var host = _configuration["Email:Smtp:Host"];
        var port = int.TryParse(_configuration["Email:Smtp:Port"], out var p) ? p : 587;
        var enableSsl = !bool.TryParse(_configuration["Email:Smtp:EnableSsl"], out var ssl) || ssl;
        var username = _configuration["Email:Smtp:Username"];
        var password = _configuration["Email:Smtp:Password"];
        var fromAddress = _configuration["Email:Smtp:FromAddress"] ?? "duilio@fariastecnologia.com.br";
        var fromName = _configuration["Email:Smtp:FromName"] ?? "Portal Corporativo ANI — Grupo Itapemirim";

        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning(
                "SmtpEmailService acionado, mas 'Email:Smtp:Host' não está configurado. E-mail para '{Destinatario}' não pôde ser despachado via rede.",
                destinatario);
            if (!alsoWriteToFile && fallbackToFileOnError)
            {
                await _fileEmailService.EnviarAsync(destinatario, assunto, corpoHtml, ct);
            }
            return;
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 15000
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

            _logger.LogInformation("Enviando e-mail via AWS SES SMTP ({Host}:{Port}) para '{Destinatario}'...", host, port, destinatario);
            await client.SendMailAsync(mailMessage, ct);
            _logger.LogInformation("E-mail enviado via AWS SES SMTP com sucesso para '{Destinatario}'.", destinatario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail via SMTP ({Host}:{Port}) para '{Destinatario}': {Message}", host, port, destinatario, ex.Message);

            if (!fallbackToFileOnError)
            {
                throw;
            }

            if (!alsoWriteToFile)
            {
                await _fileEmailService.EnviarAsync(destinatario, assunto, corpoHtml, ct);
            }
        }
    }
}
