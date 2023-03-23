using Intersect.Config;
using Intersect.Server.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SimpleIdServer.IdServer.Store;

namespace Intersect.Server.Identity.Database;

// public sealed class DbContextOptionsRemapper<TFromContext, TToContext> : DbContextOptions<TFromContext>
//     where TFromContext : DbContext where TToContext : DbContext
// {
//     private readonly DbContextOptions<TFromContext> _fromDbContextOptions;
//
//     public DbContextOptionsRemapper(DbContextOptions<TFromContext> fromDbContextOptions) : base(fromDbContextOptions)
//     {
//         _fromDbContextOptions = fromDbContextOptions;
//     }
//
//     public override Type ContextType => _fromDbContextOptions.ContextType;
//
//     public override DbContextOptions WithExtension<TExtension>(TExtension extension) =>
//         _fromDbContextOptions.WithExtension(extension);
//
//     public DbContextOptions<TToContext> Remap() =>
//         new DbContextOptions<TToContext>(this.ExtensionsMap.ToDictionary(p => p.Key, p => p.Value.Extension));
// }

public static class DatabaseContextOptionsExtensions
{
    public static DbContextOptions<StoreDbContext> ToStoreDbContextOptions(
        this DatabaseContextOptions databaseContextOptions
    )
    {
        var queryTrackingBehavior = databaseContextOptions.QueryTrackingBehavior ??
                                    (databaseContextOptions.ReadOnly && !databaseContextOptions.ExplicitLoad
                                        ? QueryTrackingBehavior.NoTracking : QueryTrackingBehavior.TrackAll);

        var builder = new DbContextOptionsBuilder<StoreDbContext>();
        builder.EnableDetailedErrors(databaseContextOptions.EnableDetailedErrors);
        builder.EnableSensitiveDataLogging(databaseContextOptions.EnableSensitiveDataLogging);
        builder.ReplaceService<IModelCacheKeyFactory, IntersectModelCacheKeyFactory>();
        builder.UseLoggerFactory(databaseContextOptions.LoggerFactory);
        builder.UseQueryTrackingBehavior(queryTrackingBehavior);

        var connectionString = databaseContextOptions.ConnectionStringBuilder.ConnectionString;
        switch (databaseContextOptions.DatabaseType)
        {
            case DatabaseOptions.DatabaseType.SQLite:
                builder.UseSqlite(connectionString);
                break;

            case DatabaseOptions.DatabaseType.MySQL:
                builder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(12), default)
                );
                break;

            default:
                throw new IndexOutOfRangeException($"Invalid DatabaseType: {databaseContextOptions.DatabaseType}");
        }

        return builder.Options;
    }
}

public class IdentityDbContext : StoreDbContext
{
    public IdentityDbContext(DatabaseContextOptions databaseContextOptions) : base(
        databaseContextOptions.ToStoreDbContextOptions()
    )
    {
    }
}