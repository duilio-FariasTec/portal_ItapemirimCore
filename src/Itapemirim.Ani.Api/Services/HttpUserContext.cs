namespace Itapemirim.Ani.Api.Services;

using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

    public long? CurrentUserId
    {
        get
        {
            var claim = HttpContext?.User.FindFirst("user_id")?.Value
                     ?? HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(claim, out var id) ? id : null;
        }
    }

    public Guid? CurrentUserGuid
    {
        get
        {
            var claim = HttpContext?.User.FindFirst("user_guid")?.Value
                     ?? HttpContext?.User.FindFirst("sub")?.Value;
            return Guid.TryParse(claim, out var guid) ? guid : null;
        }
    }

    public string? IpAddress =>
        HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
        ?? HttpContext?.Connection.RemoteIpAddress?.ToString();

    public Guid? CorrelationId
    {
        get
        {
            if (HttpContext?.Items.TryGetValue("CorrelationId", out var id) == true && id is Guid guid)
                return guid;

            var header = HttpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault();
            return Guid.TryParse(header, out var parsed) ? parsed : null;
        }
    }
}