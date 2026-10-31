namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.DTOs.Permissoes;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;

public class PermissaoService : IPermissaoService
{
    private readonly IPermissaoRepository _permissaoRepo;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public PermissaoService(IPermissaoRepository permissaoRepo, IUnitOfWork uow, IAuditoriaService auditoria)
    {
        _permissaoRepo = permissaoRepo;
        _uow = uow;
        _auditoria = auditoria;
    }

    public async Task<IEnumerable<PermissaoResponse>> ListarAsync(bool apenasAtivas = false, CancellationToken ct = default)
    {
        var permissoes = await _permissaoRepo.GetAllAsync(apenasAtivas, ct);
        return permissoes.Select(MapearParaResponse);
    }

    public async Task<PermissaoResponse> ObterPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var permissao = await _permissaoRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("PermissÃ£o", guid);

        return MapearParaResponse(permissao);
    }

    public async Task<PermissaoResponse> CadastrarAsync(PermissaoCreateRequest request, CancellationToken ct = default)
    {
        if (await _permissaoRepo.ExistsCodigoAsync(request.Codigo, null, ct))
            throw new RegraNegocioException($"O cÃ³digo de permissÃ£o '{request.Codigo}' jÃ¡ existe.");

        var permissao = new Permissao(request.Codigo, request.Nome, request.Descricao, request.Recurso, request.Acao, request.DscUrl, request.Ativo);
        await _permissaoRepo.AddAsync(permissao, ct);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.INCLUSAO, "Permissao", permissao.PermissaoGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return MapearParaResponse(permissao);
    }

    public async Task<PermissaoResponse> AtualizarAsync(Guid guid, PermissaoUpdateRequest request, CancellationToken ct = default)
    {
        var permissao = await _permissaoRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("PermissÃ£o", guid);

        permissao.Atualizar(request.Nome, request.Descricao, request.Recurso, request.Acao, request.DscUrl, request.Ativo);
        _permissaoRepo.Update(permissao);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "Permissao", permissao.PermissaoGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return MapearParaResponse(permissao);
    }

    public async Task InativarAsync(Guid guid, CancellationToken ct = default)
    {
        var permissao = await _permissaoRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("PermissÃ£o", guid);

        permissao.Desativar();
        _permissaoRepo.Update(permissao);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.REMOCAO, "Permissao", permissao.PermissaoGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    private static PermissaoResponse MapearParaResponse(Permissao p) =>
        new(p.PermissaoGuid, p.Codigo, p.Nome, p.Descricao, p.Recurso, p.Acao, p.DscUrl, p.Ativo, p.CriadoEmUtc, p.AtualizadoEmUtc);
}