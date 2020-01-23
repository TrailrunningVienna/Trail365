using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Tasks;
using Trail365.Web.Tasks;

namespace Trail365.Web.Controllers
{
    public class TrailPreviewController : Controller
    {
        private readonly TrailContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBackgroundTaskQueue _queue;

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult CalculateTrailPreview(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            Trail trail = null;

            using (var t = _context.DependencyTracker(nameof(this.CalculateTrailPreview)))
            {
                trail = _context.Trails.Find(id);
            }

            if (trail == null)
            {
                return this.NotFound();
            }
            var task = BackgroundTaskFactory.CreateTask<TrailPreviewTask>(this._serviceScopeFactory, this.Url);
            task.Trail = trail;
            task.Queue(this._queue);

            this.TempData["Info"] = $"Vorschau Berechnung für '{trail.Name}' wurde im Hintergrund gestartet! Das Ergebnis sollte in wenigen Minuten für alle User sichtbar sein.";

            return this.RedirectToAction("Trail", "TrailDetails", new { id = trail.ID });
        }

        public TrailPreviewController(TrailContext context, IServiceScopeFactory serviceScopeFactory, IBackgroundTaskQueue queue)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }
    }
}
