namespace Itapemirim.Ani.Domain.Entities;

public class Permissao
{
    public long PermissaoId { get; private set; }
    public Guid PermissaoGuid { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public string Recurso { get; private set; } = null!;
    public string Acao { get; private set; } = null!;
    public string? DscUrl { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEmUtc { get; private set; }
    public DateTime? AtualizadoEmUtc { get; private set; }

    public ICollection<PerfilPermissao> Perfis { get; private set; } = new List<PerfilPermissao>();

    protected Permissao() { }

    public Permissao(string codigo, string nome, string? descricao, string recurso, string acao, string? dscUrl = null, bool ativo = true)
    {
        PermissaoGuid = Guid.NewGuid();
        Codigo = codigo;
        Nome = nome;
        Descricao = descricao;
        Recurso = recurso;
        Acao = acao;
        DscUrl = dscUrl;
        Ativo = ativo;
        CriadoEmUtc = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string? descricao, string recurso, string acao, string? dscUrl, bool ativo)
    {
        Nome = nome;
        Descricao = descricao;
        Recurso = recurso;
        Acao = acao;
        DscUrl = dscUrl;
        Ativo = ativo;
        AtualizadoEmUtc = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        AtualizadoEmUtc = DateTime.UtcNow;
    }
}