namespace Itapemirim.Ani.Application.Interfaces;

public interface IEmailService
{
    Task EnviarAsync(string destinatario, string assunto, string corpoHtml, CancellationToken ct = default);
}

