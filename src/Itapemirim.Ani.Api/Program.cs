using Itapemirim.Ani.Api.Middlewares;
using Itapemirim.Ani.Api.Services;
using Itapemirim.Ani.Application;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Infrastructure;
using Itapemirim.Ani.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

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

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ANI Core v1");
        c.RoutePrefix = string.Empty;
    });

    // Garante que o banco SQLite seja criado automaticamente no primeiro run
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AniDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();