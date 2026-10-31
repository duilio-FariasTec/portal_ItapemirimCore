namespace Itapemirim.Ani.Domain.Entities;

public class PerfilPermissao
{
    public long PerfilPermissaoId { get; private set; }
    public Guid PerfilPermissaoGuid { get; private set; }
    public long PerfilId { get; private set; }
    public long PermissaoId { get; private set; }
    public long? ConcedidoPorUsuarioId { get; private set; }
    public DateTime ConcedidoEmUtc { get; private set; }
    public long? RevogadoPorUsuarioId { get; private set; }
    public DateTime? RevogadoEmUtc { get; private set; }

    public Perfil Perfil { get; private set; } = null!;
    public Permissao Permissao { get; private set; } = null!;
    public Usuario? ConcedidoPor { get; private set; }
    public Usuario? RevogadoPor { get; private set; }

    public bool EstaAtivo => RevogadoEmUtc == null;

    protected PerfilPermissao() { }

    public PerfilPermissao(long perfilId, long permissaoId, long? concedidoPorUsuarioId)
    {
        PerfilPermissaoGuid = Guid.NewGuid();
        PerfilId = perfilId;
        PermissaoId = permissaoId;
        ConcedidoPorUsuarioId = concedidoPorUsuarioId;
        ConcedidoEmUtc = DateTime.UtcNow;
    }

    public void Revogar(long? revogadoPorUsuarioId)
    {
        RevogadoPorUsuarioId = revogadoPorUsuarioId;
        RevogadoEmUtc = DateTime.UtcNow;
    }
}