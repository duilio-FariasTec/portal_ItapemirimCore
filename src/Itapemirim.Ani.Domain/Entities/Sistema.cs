namespace Itapemirim.Ani.Domain.Entities;

public class Sistema
{
    public long SistemaId { get; private set; }
    public Guid SistemaGuid { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEmUtc { get; private set; }
    public DateTime? AtualizadoEmUtc { get; private set; }

    public ICollection<Perfil> Perfis { get; private set; } = new List<Perfil>();
    public ICollection<UsuarioPerfil> Usuarios { get; private set; } = new List<UsuarioPerfil>();

    protected Sistema() { }

    public Sistema(string codigo, string nome, string? descricao, bool ativo = true)
    {
        SistemaGuid = Guid.NewGuid();
        Codigo = codigo.Trim().ToUpperInvariant();
        Nome = nome;
        Descricao = descricao;
        Ativo = ativo;
        CriadoEmUtc = DateTime.UtcNow;
    }

    public void Atualizar(string codigo, string nome, string? descricao, bool ativo)
    {
        Codigo = codigo.Trim().ToUpperInvariant();
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
