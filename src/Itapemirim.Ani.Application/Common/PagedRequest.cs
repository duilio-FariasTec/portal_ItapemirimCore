namespace Itapemirim.Ani.Application.Common;

public record PagedRequest(int PageNumber = 1, int PageSize = 20);