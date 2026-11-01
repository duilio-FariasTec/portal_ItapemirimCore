namespace Itapemirim.Ani.Application;

using FluentValidation;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Application.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<ISistemaService, SistemaService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IPerfilService, PerfilService>();
        services.AddScoped<IPermissaoService, PermissaoService>();
        services.AddScoped<IAuthInternoService, AuthInternoService>();
        services.AddScoped<AuthService>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();

        return services;
    }
}