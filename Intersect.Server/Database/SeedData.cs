using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace Intersect.Server.Database
{

    public abstract partial class SeedData<TType> where TType : class
    {

        public void SeedIfEmpty(ISeedableContext context)
        {
            var dbSet = context.GetDbSet<TType>();

            if (dbSet == null)
            {
                return;
            }

            if (dbSet.Any())
            {
                return;
            }

            Seed(dbSet);
        }

        public abstract void Seed(DbSet<TType> dbSet);

    }

}
