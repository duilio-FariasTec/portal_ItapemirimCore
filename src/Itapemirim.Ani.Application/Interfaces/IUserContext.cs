namespace Itapemirim.Ani.Application.Interfaces;

public interface IUserContext
{
    long? CurrentUserId { get; }
    Guid? CurrentUserGuid { get; }
    string? IpAddress { get; }
    Guid? CorrelationId { get; }
}