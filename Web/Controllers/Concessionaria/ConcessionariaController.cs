using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers
{
    public class ConcessionariaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}