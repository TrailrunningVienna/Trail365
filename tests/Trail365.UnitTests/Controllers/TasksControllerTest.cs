using Microsoft.Extensions.Logging;
using Trail365.Entities;
using Trail365.UnitTests.TestContext;
using Trail365.ViewModels;
using Xunit;

namespace Trail365.UnitTests.Controllers
{
    [Trait("Category", "BuildVerification")]
    public class TasksControllerTest
    {
        [Theory]
        [InlineData(null, null, null, 3)]
        [InlineData(null, LogLevel.Trace, null, 1)]
        [InlineData(null, null, "CRIT", 1)]
        [InlineData(null, null, "ritic", 1)]
        [InlineData(null, LogLevel.Information, "CRIT", 0)]
        [InlineData("CAT003", null, null, 1)]
        public void ShouldIndexForSearch(string category, LogLevel? level, string searchText, int expectedResults)
        {
            using (var host = TestHostBuilder.DefaultForBackend().WithTaskContext().Build())
            {
                //Seeding
                var tl1 = new TaskLogItem
                {
                    LogMessage = "my CRITICAL log message",
                    Level = LogLevel.Critical.ToString(),
                    Category = "CAT001"
                };

                var tl2 = new TaskLogItem
                {
                    LogMessage = "my TRACE log message",
                    Level = LogLevel.Trace.ToString(),
                    Category = "CAT002"
                };

                var tl3 = new TaskLogItem
                {
                    LogMessage = "my information log message",
                    Level = LogLevel.Information.ToString(),
                    Category = "CAT003"
                };

                host.TaskContext.TaskLogItems.AddRange(new TaskLogItem[] { tl1, tl2, tl3 });
                host.TaskContext.SaveChanges();

                var controller = host.CreateBackendTasksController();
                var input = new TasksBackendIndexViewModel
                {
                    LogLevel = level,
                    Category = category,
                    SearchText = searchText
                };
                var result = controller.Index(input).ToModel<TasksBackendIndexViewModel>();
                Assert.Equal(expectedResults, result.LogItems.Count);
            }
        }
    }
}
