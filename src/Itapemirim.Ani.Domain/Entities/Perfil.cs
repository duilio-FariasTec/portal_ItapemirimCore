namespace Itapemirim.Ani.Domain.Entities;

public class Perfil
{
    public long PerfilId { get; private set; }
    public Guid PerfilGuid { get; private set; }
    public long SistemaId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEmUtc { get; private set; }
    public DateTime? AtualizadoEmUtc { get; private set; }

    public Sistema Sistema { get; private set; } = null!;
    public ICollection<UsuarioPerfil> Usuarios { get; private set; } = new List<UsuarioPerfil>();
    public ICollection<PerfilPermissao> Permissoes { get; private set; } = new List<PerfilPermissao>();

    protected Perfil() { }

    public Perfil(long sistemaId, string nome, string? descricao, bool ativo = true)
    {
        PerfilGuid = Guid.NewGuid();
        SistemaId = sistemaId;
        Nome = nome;
        Descricao = descricao;
        Ativo = ativo;
        CriadoEmUtc = DateTime.UtcNow;
    }

    public void Atualizar(long sistemaId, string nome, string? descricao, bool ativo)
    {
        SistemaId = sistemaId;
        Nome = nome;
        Descricao = descricao;
        Ativo = ativo;
        AtualizadoEmUtc = DateTime.UtcNow;
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