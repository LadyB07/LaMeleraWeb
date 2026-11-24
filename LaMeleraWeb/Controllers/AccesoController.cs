using LaMeleraWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaMeleraWeb.Controllers
{
    public class AccesoController : Controller
    {
        private readonly MeleraDAL _dal;

        public AccesoController(MeleraDAL dal)
        {
            _dal = dal;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Si el dueño ya inició sesión, lo mandamos directo al panel principal
            if (HttpContext.Session.GetInt32("IdUsuario") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string nombreUsuario, string clave)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(clave))
            {
                ViewBag.Error = "Por favor, ingrese su usuario y contraseña.";
                return View();
            }

            Usuario? usuario = _dal.ValidarUsuario(nombreUsuario, clave);

            if (usuario != null)
            {
                HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
                HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
                HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto);
                HttpContext.Session.SetString("Rol", usuario.Rol);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "No se encontró el usuario o la contraseña es incorrecta.";
            return View();
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Acceso");
        }
    }
}