using Microsoft.EntityFrameworkCore;

namespace Intersect.Server.Database;

// ReSharper disable once UnusedTypeParameter
public sealed class DatabaseContextOptionsProvider<TContext> where TContext : DbContext
{
    public DatabaseContextOptions Options { get; }

    public DatabaseContextOptionsProvider(DatabaseContextOptions databaseContextOptions)
    {
        Options = databaseContextOptions;
    }
}