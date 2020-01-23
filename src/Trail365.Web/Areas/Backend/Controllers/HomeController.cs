using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Trail365.Web.Backend.Controllers
{
    [Authorize(Roles = BackendSetup.Roles)]
    [Area("Backend")]
    [Route("[area]/[controller]/[action]")]
    public partial class HomeController : Controller
    {
        public IActionResult Index()
        {
            return this.View();
        }
    }
}
