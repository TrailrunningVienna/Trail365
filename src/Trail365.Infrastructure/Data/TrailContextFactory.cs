using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Trail365.Data
{
    public class TrailContextFactory : IDesignTimeDbContextFactory<TrailContext>
    {
        public TrailContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TrailContext>();
            optionsBuilder.UseSqlite("Data Source=:memory:;New=True;");
            return new TrailContext(optionsBuilder.Options);
        }
    }
}
