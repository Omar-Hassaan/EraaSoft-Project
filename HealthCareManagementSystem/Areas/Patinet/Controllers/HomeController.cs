using Microsoft.AspNetCore.Mvc;

namespace HealthCareManagementSystem.Areas.Patinet.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
