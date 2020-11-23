using Microsoft.EntityFrameworkCore.Infrastructure;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Intersect.Server.Database
{
    public interface IDbContext : IDisposable
    {
        DatabaseFacade Database { get; }

        int SaveChanges();

        int SaveChanges(bool acceptAllChangesOnSuccess);

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
