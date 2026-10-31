namespace Itapemirim.Ani.Application.Interfaces;

using Itapemirim.Ani.Application.Common;
using Itapemirim.Ani.Application.DTOs.Auditoria;
using Itapemirim.Ani.Domain.Enums;

public interface IAuditoriaService
{
    Task<PagedResult<AuditoriaResponse>> ListarAsync(AuditoriaFiltroRequest request, CancellationToken ct = default);
    Task<AuditoriaResponse?> ObterPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task RegistrarAsync(TipoAcaoAuditoria acao, string entidade, string entidadeId, ResultadoAuditoria resultado, CancellationToken ct = default);
}