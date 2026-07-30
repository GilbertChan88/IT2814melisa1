using Microsoft.AspNetCore.Mvc;

namespace ArcaneVault_WebAPI.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
