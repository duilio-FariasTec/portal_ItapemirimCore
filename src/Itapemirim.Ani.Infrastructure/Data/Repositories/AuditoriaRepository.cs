namespace Itapemirim.Ani.Infrastructure.Data.Repositories;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly AniDbContext _db;

    public AuditoriaRepository(AniDbContext db) => _db = db;

    public async Task<Auditoria?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
    {
        return await _db.Auditorias
            .Include(a => a.Ator)
            .FirstOrDefaultAsync(a => a.AuditoriaGuid == guid, ct);
    }

    public async Task<(IEnumerable<Auditoria> Itens, int Total)> GetPagedAsync(
        Guid? atorGuid,
        string? acao,
        string? entidade,
        string? resultado,
        DateTime? dataInicio,
        DateTime? dataFim,
        Guid? correlationId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Auditorias
            .Include(a => a.Ator)
            .AsNoTracking()
            .AsQueryable();

        if (atorGuid.HasValue)
            query = query.Where(a => a.Ator != null && a.Ator.UsuarioGuid == atorGuid.Value);

        if (!string.IsNullOrWhiteSpace(acao))
            query = query.Where(a => a.Acao.ToString() == acao.ToUpper());

        if (!string.IsNullOrWhiteSpace(entidade))
            query = query.Where(a => a.Entidade.ToLower() == entidade.ToLower());

        if (!string.IsNullOrWhiteSpace(resultado))
            query = query.Where(a => a.Resultado.ToString() == resultado.ToUpper());

        if (dataInicio.HasValue)
            query = query.Where(a => a.OcorridoEmUtc >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(a => a.OcorridoEmUtc <= dataFim.Value);

        if (correlationId.HasValue)
            query = query.Where(a => a.CorrelationId == correlationId.Value);

        var total = await query.CountAsync(ct);

        var skip = (pageNumber - 1) * pageSize;
        var itens = await query
            .OrderByDescending(a => a.OcorridoEmUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return (itens, total);
    }

    public async Task AddAsync(Auditoria auditoria, CancellationToken ct = default)
    {
        await _db.Auditorias.AddAsync(auditoria, ct);
    }
}