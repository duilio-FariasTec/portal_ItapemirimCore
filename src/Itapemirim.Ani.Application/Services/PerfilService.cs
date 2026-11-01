namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.DTOs.Perfis;
using Itapemirim.Ani.Application.DTOs.Permissoes;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;

public class PerfilService : IPerfilService
{
    private readonly IPerfilRepository _perfilRepo;
    private readonly ISistemaRepository _sistemaRepo;
    private readonly IPermissaoRepository _permissaoRepo;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;
    private readonly IUserContext _userContext;

    public PerfilService(
        IPerfilRepository perfilRepo,
        ISistemaRepository sistemaRepo,
        IPermissaoRepository permissaoRepo,
        IUnitOfWork uow,
        IAuditoriaService auditoria,
        IUserContext userContext)
    {
        _perfilRepo = perfilRepo;
        _sistemaRepo = sistemaRepo;
        _permissaoRepo = permissaoRepo;
        _uow = uow;
        _auditoria = auditoria;
        _userContext = userContext;
    }

    public async Task<IEnumerable<PerfilResponse>> ListarAsync(bool apenasAtivos = false, Guid? sistemaGuid = null, CancellationToken ct = default)
    {
        long? sistemaId = null;
        if (sistemaGuid.HasValue)
        {
            var sistema = await _sistemaRepo.GetByGuidAsync(sistemaGuid.Value, ct);
            sistemaId = sistema?.SistemaId;
        }

        var perfis = await _perfilRepo.GetAllAsync(apenasAtivos, sistemaId, ct);
        return perfis.Select(p => new PerfilResponse(
            p.PerfilGuid,
            p.Sistema?.SistemaGuid ?? Guid.Empty,
            p.Sistema?.Codigo ?? string.Empty,
            p.Sistema?.Nome ?? string.Empty,
            p.Nome,
            p.Descricao,
            p.Ativo,
            p.CriadoEmUtc,
            p.AtualizadoEmUtc
        ));
    }

    public async Task<PerfilResponse> ObterPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", guid);

        return new PerfilResponse(
            perfil.PerfilGuid,
            perfil.Sistema?.SistemaGuid ?? Guid.Empty,
            perfil.Sistema?.Codigo ?? string.Empty,
            perfil.Sistema?.Nome ?? string.Empty,
            perfil.Nome,
            perfil.Descricao,
            perfil.Ativo,
            perfil.CriadoEmUtc,
            perfil.AtualizadoEmUtc
        );
    }

    public async Task<PerfilResponse> CriarAsync(PerfilCreateRequest request, CancellationToken ct = default)
    {
        var sistema = await _sistemaRepo.GetByGuidAsync(request.SistemaId, ct)
            ?? throw new EntidadeNaoEncontradaException("Sistema", request.SistemaId);

        if (await _perfilRepo.ExistsNomeNoSistemaAsync(request.Nome, sistema.SistemaId, null, ct))
            throw new ConflitoException($"Já existe um perfil com o nome '{request.Nome}' cadastrado no sistema '{sistema.Nome}'.");

        var perfil = new Perfil(sistema.SistemaId, request.Nome, request.Descricao, request.Ativo);
        await _perfilRepo.AddAsync(perfil, ct);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.INCLUSAO, "Perfil", perfil.PerfilGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return new PerfilResponse(
            perfil.PerfilGuid,
            sistema.SistemaGuid,
            sistema.Codigo,
            sistema.Nome,
            perfil.Nome,
            perfil.Descricao,
            perfil.Ativo,
            perfil.CriadoEmUtc,
            perfil.AtualizadoEmUtc
        );
    }

    public async Task<PerfilResponse> AtualizarAsync(Guid guid, PerfilUpdateRequest request, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", guid);

        var sistema = perfil.Sistema;
        if (request.SistemaId.HasValue && request.SistemaId.Value != perfil.Sistema.SistemaGuid)
        {
            sistema = await _sistemaRepo.GetByGuidAsync(request.SistemaId.Value, ct)
                ?? throw new EntidadeNaoEncontradaException("Sistema", request.SistemaId.Value);
        }

        if (await _perfilRepo.ExistsNomeNoSistemaAsync(request.Nome, sistema.SistemaId, perfil.PerfilId, ct))
            throw new ConflitoException($"Já existe outro perfil com o nome '{request.Nome}' no sistema '{sistema.Nome}'.");

        perfil.Atualizar(sistema.SistemaId, request.Nome, request.Descricao, request.Ativo);
        _perfilRepo.Update(perfil);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "Perfil", perfil.PerfilGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return new PerfilResponse(
            perfil.PerfilGuid,
            sistema.SistemaGuid,
            sistema.Codigo,
            sistema.Nome,
            perfil.Nome,
            perfil.Descricao,
            perfil.Ativo,
            perfil.CriadoEmUtc,
            perfil.AtualizadoEmUtc
        );
    }

    public async Task InativarAsync(Guid guid, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", guid);

        perfil.Desativar();
        _perfilRepo.Update(perfil);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.REMOCAO, "Perfil", perfil.PerfilGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    public async Task<IEnumerable<PermissaoResponse>> ObterPermissoesAsync(Guid perfilGuid, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(perfilGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", perfilGuid);

        return perfil.Permissoes
            .Where(p => p.EstaAtivo && p.Permissao.Ativo)
            .Select(p => new PermissaoResponse(
                p.Permissao.PermissaoGuid,
                p.Permissao.Codigo,
                p.Permissao.Nome,
                p.Permissao.Descricao,
                p.Permissao.Recurso,
                p.Permissao.Acao,
                p.Permissao.DscUrl,
                p.Permissao.Ativo,
                p.Permissao.CriadoEmUtc,
                p.Permissao.AtualizadoEmUtc
            ));
    }

    public async Task AssociarPermissaoAsync(Guid perfilGuid, Guid permissaoGuid, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(perfilGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", perfilGuid);

        var permissao = await _permissaoRepo.GetByGuidAsync(permissaoGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Permissão", permissaoGuid);

        var jaAssociada = perfil.Permissoes.Any(p => p.PermissaoId == permissao.PermissaoId && p.EstaAtivo);
        if (jaAssociada)
            throw new ConflitoException("A permissão já está vinculada ativamente a este perfil.");

        var vinculo = new PerfilPermissao(perfil.PerfilId, permissao.PermissaoId, _userContext.CurrentUserId);
        perfil.Permissoes.Add(vinculo);

        await _uow.CommitAsync(ct);
        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.INCLUSAO, "PerfilPermissao", vinculo.PerfilPermissaoGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    public async Task RevogarPermissaoAsync(Guid perfilGuid, Guid permissaoGuid, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(perfilGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", perfilGuid);

        var permissao = await _permissaoRepo.GetByGuidAsync(permissaoGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Permissão", permissaoGuid);

        var vinculo = perfil.Permissoes.FirstOrDefault(p => p.PermissaoId == permissao.PermissaoId && p.EstaAtivo)
            ?? throw new RegraNegocioException("A permissão não está vinculada ativamente a este perfil.");

        vinculo.Revogar(_userContext.CurrentUserId);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.REMOCAO, "PerfilPermissao", vinculo.PerfilPermissaoGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }
}