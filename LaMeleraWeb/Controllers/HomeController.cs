using LaMeleraWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaMeleraWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly MeleraDAL _dal;

        public HomeController(MeleraDAL dal)
        {
            _dal = dal;
        }

        public IActionResult Index()
        {
            // Proteger la ruta: si no ha iniciado sesión, regresa al Login
            if (HttpContext.Session.GetInt32("IdUsuario") == null)
                return RedirectToAction("Login", "Acceso");

            // Traemos el histórico completo desde 1980 hasta hoy para el balance global
            var movimientos = _dal.ObtenerReporte(new DateTime(1980, 1, 1), DateTime.Today.AddYears(1), null, null);

            ViewBag.TotalIngresos = movimientos.Sum(m => m.Ingreso);
            ViewBag.TotalGastos = movimientos.Sum(m => m.Gasto);
            ViewBag.SaldoDisponible = ViewBag.TotalIngresos - ViewBag.TotalGastos;

            return View(movimientos.Take(10).ToList());
        }

        public IActionResult AcercaDe()
        {
            if (HttpContext.Session.GetInt32("IdUsuario") == null)
                return RedirectToAction("Login", "Acceso");

            return View();
        }
    }
}