using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Trail365.Data
{
    public class TaskContextFactory : IDesignTimeDbContextFactory<TaskContext>
    {
        public TaskContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TaskContext>();
            optionsBuilder.UseSqlite("Data Source=:memory:;New=True;");
            return new TaskContext(optionsBuilder.Options);
        }
    }
}
