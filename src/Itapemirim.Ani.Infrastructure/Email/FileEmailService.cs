namespace Itapemirim.Ani.Infrastructure.Email;

using System.Text;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class FileEmailService : IEmailService
{
    private readonly ILogger<FileEmailService> _logger;
    private readonly string _outputDirectory;

    public FileEmailService(IConfiguration configuration, ILogger<FileEmailService> logger)
    {
        _logger = logger;
        var configuredDir = configuration["Email:FileOutputDirectory"];
        _outputDirectory = !string.IsNullOrWhiteSpace(configuredDir)
            ? Path.GetFullPath(configuredDir, Directory.GetCurrentDirectory())
            : Path.Combine(Directory.GetCurrentDirectory(), "emails");
    }

    public async Task EnviarAsync(string destinatario, string assunto, string corpoHtml, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_outputDirectory);

        var defaultFilePath = Path.Combine(_outputDirectory, "resetSenha.html");
        var timestampedFilePath = Path.Combine(_outputDirectory, $"resetSenha_{DateTime.UtcNow:yyyyMMdd_HHmmss}.html");

        await File.WriteAllTextAsync(defaultFilePath, corpoHtml, Encoding.UTF8, ct);
        await File.WriteAllTextAsync(timestampedFilePath, corpoHtml, Encoding.UTF8, ct);

        _logger.LogInformation(
            "E-mail simulado (FileEmailService) gravado com sucesso para '{Destinatario}' com assunto '{Assunto}'. Arquivo principal: {FilePath}",
            destinatario,
            assunto,
            defaultFilePath);
    }
}

