using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Trail365.Configuration;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Internal;
using Trail365.ViewModels;

namespace Trail365.Web.Backend.Controllers
{
    [Area("Backend")]
    [Route("[area]/[controller]/[action]")]
    [Authorize(Roles = BackendSetup.Roles)]
    public partial class TasksController : Controller
    {
        private readonly TaskContext _context;

        private readonly AppSettings _settings;

        public TasksController(TaskContext context, IOptionsMonitor<AppSettings> settingsMonitor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
            _settings = settingsMonitor.CurrentValue;
        }

        // GET: Backend/Places
        public IActionResult Index(TasksBackendIndexViewModel model)
        {
            if (model == null)
            {
                model = new TasksBackendIndexViewModel();
            }

            model.PageController = "Tasks";
            model.PageSize = _settings.PageSize;
            model.EnablePaging = true;
            model.PageAction = nameof(Index);

            IQueryable<TaskLogItem> rawQuery = _context.TaskLogItems;

            //WM 01/2019 with EF Core 3.1 the Contain Statement throws exception to clarify that executed on the client side
            //we execute this explicitely on the client side!

            if (string.IsNullOrEmpty(model.SearchText) == false)
            {
                var splits = model.SearchText.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var searchtextpart in splits)
                {
                    rawQuery = rawQuery.Where(e => e.LogMessage.ToLower().Contains(searchtextpart));
                }
            }

            if (model.LogLevel.HasValue)
            {
                rawQuery = rawQuery.Where(tl => tl.Level == model.LogLevel.ToString());
            }

            if (!string.IsNullOrEmpty(model.Category))
            {
                rawQuery = rawQuery.Where(tl => tl.Category == model.Category);
            }

            var data = rawQuery.OrderByDescending(i => i.TimestampUtc);

            if (model.EnablePaging)
            {
                using (var tracker = _context.DependencyTracker("TasksController.Index.Count"))
                {
                    model.UnpagedResults = data.Count();
                }
            }

            TaskLogItem[] items = null;
            using (var tracker = _context.DependencyTracker("TasksController.Index.Query"))
            {
                items = data.Skip(model.SkipEntries).Take(model.PageSize).ToArray();
            }

            Guard.AssertNotNull(items);

            model.LogItems = items.ToList();
            model.Page = Math.Max(model.Page, 1);

            var categories = _context.GetLogCategries();

            this.ViewData.CreateLogLevelSelectList("FilterLogLevel", model.LogLevel);
            this.ViewData.CreateLogCategorySelectList("FilterCategory", categories, model.Category);
            return this.View(model);
        }

        // GET: Backend/Tasks/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var item = _context.TaskLogItems.FirstOrDefault(t => t.ID == id);
            if (item == null)
            {
                return this.NotFound();
            }
            return this.View(item);
        }

        // POST: Backend/Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var item = _context.TaskLogItems.FirstOrDefault(t => t.ID == id);
            if (item == null)
            {
                return this.NotFound();
            }
            _context.DeleteTaskLogItem(item);
            _context.SaveChanges();
            this.TempData["Info"] = "TaskLog item deleted";
            return this.RedirectToAction(nameof(Index));
        }

        // GET: Backend/Tasks/Edit/5
        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var item = _context.TaskLogItems.FirstOrDefault(t => t.ID == id);
            if (item == null)
            {
                return this.NotFound();
            }
            return this.View(item);
        }
    }
}
