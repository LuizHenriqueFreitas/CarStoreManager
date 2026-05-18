using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[Authorize(Roles = "Admin,ChefeOficina,Mecanico")]
public class OficinaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
