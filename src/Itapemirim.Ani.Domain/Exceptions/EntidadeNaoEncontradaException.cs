namespace Itapemirim.Ani.Domain.Exceptions;

public class EntidadeNaoEncontradaException : DomainException
{
    public EntidadeNaoEncontradaException(string entidade, object id)
        : base($"Entidade '{entidade}' com identificador '{id}' nÃ£o foi encontrada.") { }
}