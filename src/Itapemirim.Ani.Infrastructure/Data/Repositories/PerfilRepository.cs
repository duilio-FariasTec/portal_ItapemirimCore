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
            .Include(p => p.Sistema)
            .Include(p => p.Permissoes)
                .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(p => p.PerfilId == id, ct);
    }

    public async Task<Perfil?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
    {
        return await _db.Perfis
            .Include(p => p.Sistema)
            .Include(p => p.Permissoes)
                .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(p => p.PerfilGuid == guid, ct);
    }

    public async Task<bool> ExistsNomeNoSistemaAsync(string nome, long sistemaId, long? ignorarPerfilId = null, CancellationToken ct = default)
    {
        var nomeTrim = nome.Trim().ToLower();
        return await _db.Perfis
            .AnyAsync(p => p.SistemaId == sistemaId
                        && p.Nome.ToLower() == nomeTrim
                        && (!ignorarPerfilId.HasValue || p.PerfilId != ignorarPerfilId.Value), ct);
    }

    public async Task<IEnumerable<Perfil>> GetAllAsync(bool apenasAtivos = false, long? sistemaId = null, CancellationToken ct = default)
    {
        var query = _db.Perfis
            .Include(p => p.Sistema)
            .AsNoTracking()
            .AsQueryable();

        if (apenasAtivos)
            query = query.Where(p => p.Ativo);

        if (sistemaId.HasValue)
            query = query.Where(p => p.SistemaId == sistemaId.Value);

        return await query.OrderBy(p => p.Sistema.Nome).ThenBy(p => p.Nome).ToListAsync(ct);
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