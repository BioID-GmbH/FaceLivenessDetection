using Microsoft.AspNetCore.Mvc;

namespace FaceLivenessDetection.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
