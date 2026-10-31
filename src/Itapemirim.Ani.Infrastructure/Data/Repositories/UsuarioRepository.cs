namespace Itapemirim.Ani.Infrastructure.Data.Repositories;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AniDbContext _db;

    public UsuarioRepository(AniDbContext db) => _db = db;

    public async Task<Usuario?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _db.Usuarios
            .Include(u => u.PerfisAssociados)
                .ThenInclude(up => up.Perfil)
                    .ThenInclude(p => p.Permissoes)
                        .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(u => u.UsuarioId == id, ct);
    }

    public async Task<Usuario?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
    {
        return await _db.Usuarios
            .Include(u => u.PerfisAssociados)
                .ThenInclude(up => up.Perfil)
                    .ThenInclude(p => p.Permissoes)
                        .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(u => u.UsuarioGuid == guid, ct);
    }

    public async Task<Usuario?> GetByLoginAsync(string login, CancellationToken ct = default)
    {
        return await _db.Usuarios
            .Include(u => u.PerfisAssociados)
                .ThenInclude(up => up.Perfil)
                    .ThenInclude(p => p.Permissoes)
                        .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(u => u.Login == login, ct);
    }

    public async Task<bool> ExistsLoginAsync(string login, long? ignorarUsuarioId = null, CancellationToken ct = default)
    {
        return await _db.Usuarios
            .AnyAsync(u => u.Login == login && (!ignorarUsuarioId.HasValue || u.UsuarioId != ignorarUsuarioId.Value), ct);
    }

    public async Task<bool> ExistsEmailAsync(string email, long? ignorarUsuarioId = null, CancellationToken ct = default)
    {
        return await _db.Usuarios
            .AnyAsync(u => u.Email == email && (!ignorarUsuarioId.HasValue || u.UsuarioId != ignorarUsuarioId.Value), ct);
    }

    public async Task<(IEnumerable<Usuario> Itens, int Total)> GetPagedAsync(
        string? busca,
        UsuarioStatus? status,
        long? perfilId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Usuarios
            .Include(u => u.PerfisAssociados)
                .ThenInclude(up => up.Perfil)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            query = query.Where(u => u.Nome.ToLower().Contains(termo)
                                  || u.Login.ToLower().Contains(termo)
                                  || (u.Email != null && u.Email.ToLower().Contains(termo)));
        }

        if (status.HasValue)
            query = query.Where(u => u.Status == status.Value);

        if (perfilId.HasValue)
            query = query.Where(u => u.PerfisAssociados.Any(p => p.PerfilId == perfilId.Value && p.RevogadoEmUtc == null));

        var total = await query.CountAsync(ct);

        var skip = (pageNumber - 1) * pageSize;
        var itens = await query
            .OrderByDescending(u => u.CriadoEmUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return (itens, total);
    }

    public async Task AddAsync(Usuario usuario, CancellationToken ct = default)
    {
        await _db.Usuarios.AddAsync(usuario, ct);
    }

    public void Update(Usuario usuario)
    {
        _db.Usuarios.Update(usuario);
    }
}