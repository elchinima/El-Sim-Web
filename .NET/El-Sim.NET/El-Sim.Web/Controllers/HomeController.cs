using El_Sim.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace El_Sim.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Plans()
        {
            return View();
        }

        public IActionResult Pass()
        {
            return View();
        }

        public IActionResult Global()
        {
            return View();
        }

        public IActionResult Wifi()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
