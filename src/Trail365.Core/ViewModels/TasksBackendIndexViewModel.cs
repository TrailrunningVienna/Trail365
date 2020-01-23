using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class TasksBackendIndexViewModel : DatapagerViewModel
    {
        public List<TaskLogItem> LogItems { get; set; }

        public string SearchText { get; set; }

        public LogLevel? LogLevel { get; set; }

        public string Category { get; set; }
    }
}
