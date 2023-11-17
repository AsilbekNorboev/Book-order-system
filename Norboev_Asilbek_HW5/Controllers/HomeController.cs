using Microsoft.AspNetCore.Mvc;

namespace Norboev_Asilbek_HW5.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}