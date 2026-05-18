using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[Authorize(Roles = "Admin,GerenteVendas,Vendedor")]
public class ConcessionariaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
