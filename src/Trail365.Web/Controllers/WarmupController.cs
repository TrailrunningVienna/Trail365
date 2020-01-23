using Microsoft.AspNetCore.Mvc;

namespace Trail365.Web.Controllers
{
    public class WarmupController : Controller
    {
        public WarmupController()
        {
        }

        /// <summary>
        /// //Don't do Migrations here because they cannot be called with Concurrency in mind (Requests can be concurrent)
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return this.Ok(new { Version = Helper.GetProductVersionFromEntryAssembly(), Process = Helper.GetProcessInfo(), UpTime = Helper.GetUptime(), StartTime = Helper.GetStartTime() });
        }
    }
}
