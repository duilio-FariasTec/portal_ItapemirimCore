namespace Itapemirim.Ani.Infrastructure;

using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Interfaces;
using Itapemirim.Ani.Infrastructure.Data;
using Itapemirim.Ani.Infrastructure.Data.Repositories;
using Itapemirim.Ani.Infrastructure.Email;
using Itapemirim.Ani.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AniDbContext>(options =>
        {
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                var sqliteConn = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : "Data Source=ani.db";
                options.UseSqlite(sqliteConn);
            }
            else
            {
                var sqlServerConn = !string.IsNullOrWhiteSpace(connectionString) 
                    ? connectionString 
                    : "Server=localhost;Database=portal_Itapemirim;Trusted_Connection=True;TrustServerCertificate=True;";
                
                options.UseSqlServer(sqlServerConn, b =>
                {
                    b.MigrationsHistoryTable("__EFMigrationsHistory", "ani");
                });
            }
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AniDbContext>());

        services.AddScoped<ISistemaRepository, SistemaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPerfilRepository, PerfilRepository>();
        services.AddScoped<IPermissaoRepository, PermissaoRepository>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // Provedor de E-mail desacoplado via Feature Flag / Configuração (File ou Smtp)
        var emailProvider = configuration["Email:Provider"] ?? "File";
        if (emailProvider.Equals("Smtp", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, FileEmailService>();
        }

        return services;
    }
}