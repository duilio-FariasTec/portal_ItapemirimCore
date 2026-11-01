using Itapemirim.Ani.Api.Middlewares;
using Itapemirim.Ani.Api.Services;
using Itapemirim.Ani.Application;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Infrastructure;
using Itapemirim.Ani.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ANI Core API — Portal Itapemirim",
        Version = "v1",
        Description = "API de Domínio do ANI Core responsável pela gestão de Usuários, Perfis, Permissões e Auditoria."
    });
});

var app = builder.Build();

// Modo CLI Offline: permite criar tabelas e inserir todos os dados padrão sem subir o servidor HTTP:
// Uso: dotnet run --project src/Itapemirim.Ani.Api -- --seed-only [--reset-db]
if (args.Any(a => a.Equals("--seed-only", StringComparison.OrdinalIgnoreCase)))
{
    var resetDb = args.Any(a => a.Equals("--reset-db", StringComparison.OrdinalIgnoreCase));
    await Itapemirim.Ani.Api.DatabaseSeeder.SeedAsync(app.Services, resetDb);
    return;
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Endpoint de Health Check (essencial para AWS App Runner, ECS, ALB e Docker)
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ANI Core v1");
        c.RoutePrefix = string.Empty;
    });

    // Garante que o banco SQLite seja criado/atualizado com o schema atualizado
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AniDbContext>();

    if (db.Database.IsSqlite())
    {
        bool schemaAtualizado = false;
        try
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Sistema';";
            db.Database.OpenConnection();
            var tabelaSistema = Convert.ToInt32(cmd.ExecuteScalar()) > 0;

            if (tabelaSistema)
            {
                cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Usuario') WHERE name='ExigeTrocaSenhaNoProximoLogin';";
                var colunaReset = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                schemaAtualizado = colunaReset;
            }
        }
        catch
        {
            // Conexão ou tabela não existente
        }
        finally
        {
            db.Database.CloseConnection();
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        }

        if (!schemaAtualizado)
        {
            db.Database.EnsureDeleted();
        }
    }

    db.Database.EnsureCreated();
}

// Redireciona HTTPS apenas fora de containers (containers rodam HTTP interno na porta 8080 com terminação TLS no Load Balancer/CloudFront)
if (!app.Environment.IsProduction() && Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();