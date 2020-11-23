using Intersect.Server.Database.Logging.Entities;

using Microsoft.EntityFrameworkCore;

namespace Intersect.Server.Database.Logging
{
    public interface ILoggingContext : IDbContext
    {
        DbSet<RequestLog> RequestLogs { get; }

        DbSet<UserActivityHistory> UserActivityHistory { get; }
    }
}
