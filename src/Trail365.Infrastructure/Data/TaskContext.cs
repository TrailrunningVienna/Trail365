using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Trail365.Entities;

namespace Trail365.Data
{
    public partial class TaskContext : DbContext, IDependencyTracker
    {
        string IDependencyTracker.OperationTarget => OperationTarget;

        string IDependencyTracker.OperationType(bool cached)
        {
            if (cached)
            {
                return "CACHE-" + OperationType;
            }
            else
            {
                return OperationType;
            }
        }

        public void DeleteTaskLogItem(TaskLogItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.Remove(entity);
        }

        public override int SaveChanges()
        {
            using (var tracker = this.DependencyTracker(nameof(SaveChanges)))
            {
                var result = base.SaveChanges();
                tracker.Telemetry.Properties.Add("StateEntriesChanged", result.ToString());
                return result;
            }
        }

        public DbSet<TaskLogItem> TaskLogItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskLogItem>();
        }

        public TaskContext(DbContextOptions<TaskContext> options) : base(options)
        {
        }

        private static readonly string OperationType = "SQLITE";
        private static readonly string OperationTarget = "TaskContext";

        public string[] GetLogCategries()
        {
            using (var tracker = this.DependencyTracker(nameof(this.GetLogCategries)))
            {
                return this.TaskLogItems.Select(tl => tl.Category).Distinct().ToArray();
            };
        }
    }
}
