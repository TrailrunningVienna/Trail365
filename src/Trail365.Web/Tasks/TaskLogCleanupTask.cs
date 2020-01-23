using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Trail365.Data;
using Trail365.Tasks;

namespace Trail365.Web.Tasks
{
    public class TaskLogCleanupTask : BackgroundTask
    {
        protected override Task Execute(CancellationToken cancellationToken)
        {
            this.TaskContext = this.Context.ServiceProvider.GetRequiredService<TaskContext>();
            this.TaskContext.TaskLogItems.RemoveRange(this.TaskContext.TaskLogItems);
            return this.TaskContext.SaveChangesAsync();
        }

        protected TaskContext TaskContext { get; set; }
    }
}
