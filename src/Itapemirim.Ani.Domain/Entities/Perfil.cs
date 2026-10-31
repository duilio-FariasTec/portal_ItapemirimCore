namespace Itapemirim.Ani.Domain.Entities;

public class Perfil
{
    public long PerfilId { get; private set; }
    public Guid PerfilGuid { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEmUtc { get; private set; }
    public DateTime? AtualizadoEmUtc { get; private set; }

    public ICollection<UsuarioPerfil> Usuarios { get; private set; } = new List<UsuarioPerfil>();
    public ICollection<PerfilPermissao> Permissoes { get; private set; } = new List<PerfilPermissao>();

    protected Perfil() { }

    public Perfil(string nome, string? descricao, bool ativo = true)
    {
        PerfilGuid = Guid.NewGuid();
        Nome = nome;
        Descricao = descricao;
        Ativo = ativo;
        CriadoEmUtc = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string? descricao, bool ativo)
    {
        Nome = nome;
        Descricao = descricao;
        Ativo = ativo;
        AtualizadoEmUtc = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        AtualizadoEmUtc = DateTime.UtcNow;
    }
}