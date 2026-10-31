namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.DTOs.Auth;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;

public class AuthInternoService : IAuthInternoService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditoriaService _auditoria;

    public AuthInternoService(IUsuarioRepository usuarioRepo, IPasswordHasher hasher, IAuditoriaService auditoria)
    {
        _usuarioRepo = usuarioRepo;
        _hasher = hasher;
        _auditoria = auditoria;
    }

    public async Task<UsuarioAutenticadoResponse> ValidarAsync(ValidarCredenciaisRequest request, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByLoginAsync(request.Login, ct);

        if (usuario == null || !_hasher.VerifyPassword(request.Senha, usuario.SenhaHash))
        {
            await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "SessaoLogin", request.Login, ResultadoAuditoria.FALHA, ct);
            throw new RegraNegocioException("Credenciais invÃ¡lidas.");
        }

        if (usuario.Status != UsuarioStatus.Ativo)
        {
            await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "SessaoLogin", request.Login, ResultadoAuditoria.FALHA, ct);
            throw new RegraNegocioException($"UsuÃ¡rio nÃ£o pode se autenticar pois estÃ¡ com status '{usuario.Status}'.");
        }

        var perfis = usuario.PerfisAssociados
            .Where(up => up.EstaAtivo && up.Perfil.Ativo)
            .Select(up => up.Perfil.Nome)
            .Distinct()
            .ToList();

        var permissoes = usuario.PerfisAssociados
            .Where(up => up.EstaAtivo && up.Perfil.Ativo)
            .SelectMany(up => up.Perfil.Permissoes)
            .Where(pp => pp.EstaAtivo && pp.Permissao.Ativo)
            .Select(pp => pp.Permissao.Codigo)
            .Distinct()
            .ToList();

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "SessaoLogin", usuario.UsuarioGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return new UsuarioAutenticadoResponse(
            usuario.UsuarioGuid,
            usuario.Nome,
            usuario.Login,
            usuario.Email,
            usuario.Status.ToString(),
            perfis,
            permissoes
        );
    }
}