namespace Itapemirim.Ani.Infrastructure.Data.Repositories;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class PermissaoRepository : IPermissaoRepository
{
    private readonly AniDbContext _db;

    public PermissaoRepository(AniDbContext db) => _db = db;

    public async Task<Permissao?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _db.Permissoes.FirstOrDefaultAsync(p => p.PermissaoId == id, ct);
    }

    public async Task<Permissao?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
    {
        return await _db.Permissoes.FirstOrDefaultAsync(p => p.PermissaoGuid == guid, ct);
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, long? ignorarPermissaoId = null, CancellationToken ct = default)
    {
        return await _db.Permissoes
            .AnyAsync(p => p.Codigo == codigo && (!ignorarPermissaoId.HasValue || p.PermissaoId != ignorarPermissaoId.Value), ct);
    }

    public async Task<IEnumerable<Permissao>> GetAllAsync(bool apenasAtivas = false, CancellationToken ct = default)
    {
        var query = _db.Permissoes.AsNoTracking().AsQueryable();
        if (apenasAtivas)
            query = query.Where(p => p.Ativo);

        return await query.OrderBy(p => p.Recurso).ThenBy(p => p.Acao).ToListAsync(ct);
    }

    public async Task AddAsync(Permissao permissao, CancellationToken ct = default)
    {
        await _db.Permissoes.AddAsync(permissao, ct);
    }

    public void Update(Permissao permissao)
    {
        _db.Permissoes.Update(permissao);
    }
}