using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}