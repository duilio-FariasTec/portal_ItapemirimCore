namespace Itapemirim.Ani.Domain.Exceptions;

public class ConflitoException : DomainException
{
    public ConflitoException(string message) : base(message) { }
}
