using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers
{
    public class OficinaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}