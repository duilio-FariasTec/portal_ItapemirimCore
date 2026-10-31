namespace Itapemirim.Ani.Domain.Interfaces;

using Itapemirim.Ani.Domain.Entities;

public interface IAuditoriaRepository
{
    Task<Auditoria?> GetByGuidAsync(Guid guid, CancellationToken ct = default);
    Task<(IEnumerable<Auditoria> Itens, int Total)> GetPagedAsync(
        Guid? atorGuid,
        string? acao,
        string? entidade,
        string? resultado,
        DateTime? dataInicio,
        DateTime? dataFim,
        Guid? correlationId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
    Task AddAsync(Auditoria auditoria, CancellationToken ct = default);
}