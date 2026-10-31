namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.Common;
using Itapemirim.Ani.Application.DTOs.Perfis;
using Itapemirim.Ani.Application.DTOs.Usuarios;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPerfilRepository _perfilRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditoriaService _auditoria;
    private readonly IUserContext _userContext;

    public UsuarioService(
        IUsuarioRepository usuarioRepo,
        IPerfilRepository perfilRepo,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IAuditoriaService auditoria,
        IUserContext userContext)
    {
        _usuarioRepo = usuarioRepo;
        _perfilRepo = perfilRepo;
        _uow = uow;
        _hasher = hasher;
        _auditoria = auditoria;
        _userContext = userContext;
    }

    public async Task<PagedResult<UsuarioResponse>> ListarAsync(UsuarioFiltroRequest request, CancellationToken ct = default)
    {
        UsuarioStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<UsuarioStatus>(request.Status, true, out var parsedStatus))
            status = parsedStatus;

        long? perfilId = null;
        if (request.PerfilGuid.HasValue)
        {
            var perfil = await _perfilRepo.GetByGuidAsync(request.PerfilGuid.Value, ct);
            perfilId = perfil?.PerfilId;
        }

        var (itens, total) = await _usuarioRepo.GetPagedAsync(request.Busca, status, perfilId, request.PageNumber, request.PageSize, ct);

        var dtos = itens.Select(MapearParaResponse).ToList();
        return new PagedResult<UsuarioResponse>(dtos, total, request.PageNumber, request.PageSize);
    }

    public async Task<UsuarioResponse> ObterPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("UsuÃ¡rio", guid);

        return MapearParaResponse(usuario);
    }

    public async Task<UsuarioResponse> CadastrarAsync(UsuarioCreateRequest request, CancellationToken ct = default)
    {
        if (await _usuarioRepo.ExistsLoginAsync(request.Login, null, ct))
            throw new RegraNegocioException($"O login '{request.Login}' jÃ¡ estÃ¡ em uso.");

        if (!string.IsNullOrWhiteSpace(request.Email) && await _usuarioRepo.ExistsEmailAsync(request.Email, null, ct))
            throw new RegraNegocioException($"O e-mail '{request.Email}' jÃ¡ estÃ¡ em uso.");

        var status = UsuarioStatus.Ativo;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<UsuarioStatus>(request.Status, true, out var parsed))
            status = parsed;

        var senhaHash = _hasher.HashPassword(request.Senha);
        var usuario = new Usuario(request.Nome, request.Login, request.Email, senhaHash, status);

        await _usuarioRepo.AddAsync(usuario, ct);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.INCLUSAO, "Usuario", usuario.UsuarioGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return MapearParaResponse(usuario);
    }

    public async Task<UsuarioResponse> AtualizarAsync(Guid guid, UsuarioUpdateRequest request, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("UsuÃ¡rio", guid);

        if (!string.IsNullOrWhiteSpace(request.Email) && await _usuarioRepo.ExistsEmailAsync(request.Email, usuario.UsuarioId, ct))
            throw new RegraNegocioException($"O e-mail '{request.Email}' jÃ¡ estÃ¡ cadastrado para outro usuÃ¡rio.");

        if (!Enum.TryParse<UsuarioStatus>(request.Status, true, out var status))
            throw new RegraNegocioException($"Status '{request.Status}' invÃ¡lido.");

        usuario.AtualizarDados(request.Nome, request.Email, status);
        _usuarioRepo.Update(usuario);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "Usuario", usuario.UsuarioGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return MapearParaResponse(usuario);
    }

    public async Task InativarAsync(Guid guid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("UsuÃ¡rio", guid);

        usuario.Inativar();
        _usuarioRepo.Update(usuario);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.REMOCAO, "Usuario", usuario.UsuarioGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    public async Task<IEnumerable<PerfilResumidoResponse>> ObterPerfisAsync(Guid guid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("UsuÃ¡rio", guid);

        return usuario.PerfisAssociados
            .Where(p => p.EstaAtivo && p.Perfil.Ativo)
            .Select(p => new PerfilResumidoResponse(p.Perfil.PerfilGuid, p.Perfil.Nome, p.ConcedidoEmUtc))
            .ToList();
    }

    public async Task AtribuirPerfilAsync(Guid usuarioGuid, Guid perfilGuid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(usuarioGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("UsuÃ¡rio", usuarioGuid);

        var perfil = await _perfilRepo.GetByGuidAsync(perfilGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", perfilGuid);

        var jaAtribuido = usuario.PerfisAssociados.Any(p => p.PerfilId == perfil.PerfilId && p.EstaAtivo);
        if (jaAtribuido)
            throw new RegraNegocioException("O usuÃ¡rio jÃ¡ possui este perfil ativo.");

        var vinculo = new UsuarioPerfil(usuario.UsuarioId, perfil.PerfilId, _userContext.CurrentUserId);
        usuario.PerfisAssociados.Add(vinculo);

        await _uow.CommitAsync(ct);
        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.INCLUSAO, "UsuarioPerfil", vinculo.UsuarioPerfilGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    public async Task RevogarPerfilAsync(Guid usuarioGuid, Guid perfilGuid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(usuarioGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("UsuÃ¡rio", usuarioGuid);

        var perfil = await _perfilRepo.GetByGuidAsync(perfilGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", perfilGuid);

        var vinculo = usuario.PerfisAssociados.FirstOrDefault(p => p.PerfilId == perfil.PerfilId && p.EstaAtivo)
            ?? throw new RegraNegocioException("O usuÃ¡rio nÃ£o possui este perfil ativo para revogar.");

        vinculo.Revogar(_userContext.CurrentUserId);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.REMOCAO, "UsuarioPerfil", vinculo.UsuarioPerfilGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    public async Task<IEnumerable<PermissaoEfetivaResponse>> ObterPermissoesEfetivasAsync(Guid usuarioGuid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(usuarioGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("UsuÃ¡rio", usuarioGuid);

        var permissoes = usuario.PerfisAssociados
            .Where(up => up.EstaAtivo && up.Perfil.Ativo)
            .SelectMany(up => up.Perfil.Permissoes)
            .Where(pp => pp.EstaAtivo && pp.Permissao.Ativo)
            .Select(pp => pp.Permissao)
            .DistinctBy(p => p.PermissaoId)
            .Select(p => new PermissaoEfetivaResponse(p.PermissaoGuid, p.Codigo, p.Nome, p.Recurso, p.Acao, p.DscUrl))
            .ToList();

        return permissoes;
    }

    private static UsuarioResponse MapearParaResponse(Usuario u) =>
        new(
            u.UsuarioGuid,
            u.Nome,
            u.Login,
            u.Email,
            u.Status.ToString(),
            u.CriadoEmUtc,
            u.AtualizadoEmUtc,
            u.PerfisAssociados
                .Where(p => p.EstaAtivo && p.Perfil.Ativo)
                .Select(p => new PerfilResumidoResponse(p.Perfil.PerfilGuid, p.Perfil.Nome, p.ConcedidoEmUtc))
                .ToList()
        );
}