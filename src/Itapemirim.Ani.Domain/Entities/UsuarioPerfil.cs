namespace Itapemirim.Ani.Domain.Entities;

public class UsuarioPerfil
{
    public long UsuarioPerfilId { get; private set; }
    public Guid UsuarioPerfilGuid { get; private set; }
    public long UsuarioId { get; private set; }
    public long PerfilId { get; private set; }
    public long? ConcedidoPorUsuarioId { get; private set; }
    public DateTime ConcedidoEmUtc { get; private set; }
    public long? RevogadoPorUsuarioId { get; private set; }
    public DateTime? RevogadoEmUtc { get; private set; }

    public Usuario Usuario { get; private set; } = null!;
    public Perfil Perfil { get; private set; } = null!;
    public Usuario? ConcedidoPor { get; private set; }
    public Usuario? RevogadoPor { get; private set; }

    public bool EstaAtivo => RevogadoEmUtc == null;

    protected UsuarioPerfil() { }

    public UsuarioPerfil(long usuarioId, long perfilId, long? concedidoPorUsuarioId)
    {
        UsuarioPerfilGuid = Guid.NewGuid();
        UsuarioId = usuarioId;
        PerfilId = perfilId;
        ConcedidoPorUsuarioId = concedidoPorUsuarioId;
        ConcedidoEmUtc = DateTime.UtcNow;
    }

    public void Revogar(long? revogadoPorUsuarioId)
    {
        RevogadoPorUsuarioId = revogadoPorUsuarioId;
        RevogadoEmUtc = DateTime.UtcNow;
    }
}