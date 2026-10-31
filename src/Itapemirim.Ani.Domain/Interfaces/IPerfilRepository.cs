namespace Itapemirim.Ani.Domain.Interfaces;

using Itapemirim.Ani.Domain.Entities;

public interface IPerfilRepository
{
    Task<Perfil?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Perfil?> GetByGuidAsync(Guid guid, CancellationToken ct = default);
    Task<bool> ExistsNomeAsync(string nome, long? ignorarPerfilId = null, CancellationToken ct = default);
    Task<IEnumerable<Perfil>> GetAllAsync(bool apenasAtivos = false, CancellationToken ct = default);
    Task AddAsync(Perfil perfil, CancellationToken ct = default);
    void Update(Perfil perfil);
}