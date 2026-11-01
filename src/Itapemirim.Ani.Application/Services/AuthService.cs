namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Alias semântico para <see cref="AuthInternoService"/> atendendo à nomenclatura AuthService.ResetarSenhaAsync(...).
/// </summary>
public class AuthService : AuthInternoService
{
    public AuthService(
        IUsuarioRepository usuarioRepo,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IAuditoriaService auditoria,
        IEmailService emailService,
        IConfiguration configuration)
        : base(usuarioRepo, uow, hasher, auditoria, emailService, configuration)
    {
    }
}

