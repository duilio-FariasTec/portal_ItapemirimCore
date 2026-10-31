namespace Itapemirim.Ani.Infrastructure.Data;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AniDbContext : DbContext, IUnitOfWork
{
    public AniDbContext(DbContextOptions<AniDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<UsuarioPerfil> UsuarioPerfis => Set<UsuarioPerfil>();
    public DbSet<PerfilPermissao> PerfilPermissoes => Set<PerfilPermissao>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (!Database.IsSqlite())
        {
            modelBuilder.HasDefaultSchema("ani");
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AniDbContext).Assembly);

        if (Database.IsSqlite())
        {
            // Ajustes automáticos para compatibilidade com SQLite (Ambiente Local)
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetSchema(null); // SQLite não suporta schemas (ex: "ani")

                // Ajusta Check Constraints: SQLite não suporta prefixo N'string' do SQL Server
                var checkConstraints = entity.GetCheckConstraints().ToList();
                foreach (var check in checkConstraints)
                {
                    var sql = check.Sql;
                    entity.RemoveCheckConstraint(check.Name);
                    if (!string.IsNullOrWhiteSpace(sql))
                    {
                        var sqliteSql = sql.Replace("N'", "'").Replace("[", "\"").Replace("]", "\"");
                        entity.AddCheckConstraint(check.Name, sqliteSql);
                    }
                }

                // Ajusta filtros de índices parciais para sintaxe compatível com SQLite
                foreach (var index in entity.GetIndexes())
                {
                    var filter = index.GetFilter();
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        var sqliteFilter = filter.Replace("[", "\"").Replace("]", "\"");
                        index.SetFilter(sqliteFilter);
                    }
                }

                // Remove defaults e tipos específicos do SQL Server
                foreach (var property in entity.GetProperties())
                {
                    if (property.GetDefaultValueSql()?.Contains("NEWSEQUENTIALID", StringComparison.OrdinalIgnoreCase) == true ||
                        property.GetDefaultValueSql()?.Contains("SYSUTCDATETIME", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        property.SetDefaultValueSql(null);
                    }

                    if (property.GetColumnType()?.Contains("datetime2", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        property.SetColumnType(null);
                    }
                }
            }
        }
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        return await SaveChangesAsync(ct);
    }
}