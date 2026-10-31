namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.Common;
using Itapemirim.Ani.Application.DTOs.Auditoria;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Interfaces;

public class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaRepository _auditoriaRepo;
    private readonly IUnitOfWork _uow;
    private readonly IUserContext _userContext;

    public AuditoriaService(IAuditoriaRepository auditoriaRepo, IUnitOfWork uow, IUserContext userContext)
    {
        _auditoriaRepo = auditoriaRepo;
        _uow = uow;
        _userContext = userContext;
    }

    public async Task<PagedResult<AuditoriaResponse>> ListarAsync(AuditoriaFiltroRequest request, CancellationToken ct = default)
    {
        var (itens, total) = await _auditoriaRepo.GetPagedAsync(
            request.AtorUsuarioId,
            request.Acao,
            request.Entidade,
            request.Resultado,
            request.DataInicioUtc,
            request.DataFimUtc,
            request.CorrelationId,
            request.PageNumber,
            request.PageSize,
            ct);

        var dtos = itens.Select(a => new AuditoriaResponse(
            a.AuditoriaGuid,
            a.Ator?.UsuarioGuid,
            a.Ator?.Nome,
            a.Ator?.Login,
            a.Acao.ToString(),
            a.Entidade,
            a.EntidadeId,
            a.Resultado.ToString(),
            a.Origem,
            a.CorrelationId,
            a.OcorridoEmUtc
        )).ToList();

        return new PagedResult<AuditoriaResponse>(dtos, total, request.PageNumber, request.PageSize);
    }

    public async Task<AuditoriaResponse?> ObterPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var a = await _auditoriaRepo.GetByGuidAsync(guid, ct);
        if (a == null) return null;

        return new AuditoriaResponse(
            a.AuditoriaGuid,
            a.Ator?.UsuarioGuid,
            a.Ator?.Nome,
            a.Ator?.Login,
            a.Acao.ToString(),
            a.Entidade,
            a.EntidadeId,
            a.Resultado.ToString(),
            a.Origem,
            a.CorrelationId,
            a.OcorridoEmUtc
        );
    }

    public async Task RegistrarAsync(
        TipoAcaoAuditoria acao,
        string entidade,
        string entidadeId,
        ResultadoAuditoria resultado,
        CancellationToken ct = default)
    {
        var registro = new Auditoria(
            _userContext.CurrentUserId,
            acao,
            entidade,
            entidadeId,
            resultado,
            _userContext.IpAddress,
            _userContext.CorrelationId
        );

        await _auditoriaRepo.AddAsync(registro, ct);
        await _uow.CommitAsync(ct);
    }
}