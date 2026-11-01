namespace Itapemirim.Ani.Infrastructure.Data.Repositories;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class SistemaRepository : ISistemaRepository
{
    private readonly AniDbContext _db;

    public SistemaRepository(AniDbContext db) => _db = db;

    public async Task<Sistema?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _db.Sistemas
            .Include(s => s.Perfis)
            .FirstOrDefaultAsync(s => s.SistemaId == id, ct);
    }

    public async Task<Sistema?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
    {
        return await _db.Sistemas
            .Include(s => s.Perfis)
            .FirstOrDefaultAsync(s => s.SistemaGuid == guid, ct);
    }

    public async Task<Sistema?> GetByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();
        return await _db.Sistemas
            .Include(s => s.Perfis)
            .FirstOrDefaultAsync(s => s.Codigo == codigoNormalizado, ct);
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, long? ignorarSistemaId = null, CancellationToken ct = default)
    {
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();
        return await _db.Sistemas
            .AnyAsync(s => s.Codigo == codigoNormalizado && (!ignorarSistemaId.HasValue || s.SistemaId != ignorarSistemaId.Value), ct);
    }

    public async Task<IEnumerable<Sistema>> GetAllAsync(bool apenasAtivos = false, CancellationToken ct = default)
    {
        var query = _db.Sistemas.AsNoTracking().AsQueryable();
        if (apenasAtivos)
            query = query.Where(s => s.Ativo);

        return await query.OrderBy(s => s.Nome).ToListAsync(ct);
    }

    public async Task AddAsync(Sistema sistema, CancellationToken ct = default)
    {
        await _db.Sistemas.AddAsync(sistema, ct);
    }

    public void Update(Sistema sistema)
    {
        _db.Sistemas.Update(sistema);
    }
}
