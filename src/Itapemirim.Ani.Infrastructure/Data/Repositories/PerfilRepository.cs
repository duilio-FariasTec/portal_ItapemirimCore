namespace Itapemirim.Ani.Infrastructure.Data.Repositories;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class PerfilRepository : IPerfilRepository
{
    private readonly AniDbContext _db;

    public PerfilRepository(AniDbContext db) => _db = db;

    public async Task<Perfil?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _db.Perfis
            .Include(p => p.Permissoes)
                .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(p => p.PerfilId == id, ct);
    }

    public async Task<Perfil?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
    {
        return await _db.Perfis
            .Include(p => p.Permissoes)
                .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(p => p.PerfilGuid == guid, ct);
    }

    public async Task<bool> ExistsNomeAsync(string nome, long? ignorarPerfilId = null, CancellationToken ct = default)
    {
        return await _db.Perfis
            .AnyAsync(p => p.Nome == nome && (!ignorarPerfilId.HasValue || p.PerfilId != ignorarPerfilId.Value), ct);
    }

    public async Task<IEnumerable<Perfil>> GetAllAsync(bool apenasAtivos = false, CancellationToken ct = default)
    {
        var query = _db.Perfis.AsNoTracking().AsQueryable();
        if (apenasAtivos)
            query = query.Where(p => p.Ativo);

        return await query.OrderBy(p => p.Nome).ToListAsync(ct);
    }

    public async Task AddAsync(Perfil perfil, CancellationToken ct = default)
    {
        await _db.Perfis.AddAsync(perfil, ct);
    }

    public void Update(Perfil perfil)
    {
        _db.Perfis.Update(perfil);
    }
}