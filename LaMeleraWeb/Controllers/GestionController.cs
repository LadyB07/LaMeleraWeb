using LaMeleraWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaMeleraWeb.Controllers
{
    public class GestionController : Controller
    {
        private readonly MeleraDAL _dal;

        public GestionController(MeleraDAL dal)
        {
            _dal = dal;
        }

        private bool EstaAutenticado() => HttpContext.Session.GetInt32("IdUsuario") != null;
        private bool EsSoloConsulta() => HttpContext.Session.GetString("Rol") == "Consulta";

        // ==========================================
        // 1. CONTROL DE CUOTAS Y MOROSIDAD POR AÑO
        // ==========================================
        [HttpGet]
        public IActionResult Cuotas(int? anio)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Acceso");

            int anioConsulta = anio ?? DateTime.Today.Year;
            ViewBag.AnioSeleccionado = anioConsulta;

            var estado = _dal.ObtenerEstadoCuotas(anioConsulta);
            return View(estado);
        }

        // ==========================================
        // 2. REPORTES POR FECHAS / APARTADO / CONCEPTO + PDF
        // ==========================================
        [HttpGet]
        public IActionResult Reportes(DateTime? fechaInicio, DateTime? fechaFin, int? idApartado, int? idConcepto)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Acceso");

            DateTime inicio = fechaInicio ?? new DateTime(DateTime.Today.Year, 1, 1);
            DateTime fin = fechaFin ?? DateTime.Today;

            ViewBag.FechaInicio = inicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fin.ToString("yyyy-MM-dd");
            ViewBag.IdApartado = idApartado;
            ViewBag.IdConcepto = idConcepto;

            ViewBag.Conceptos = idApartado.HasValue ? _dal.ListarConceptos(idApartado.Value) : new List<Concepto>();

            var movimientos = _dal.ObtenerReporte(inicio, fin, idApartado, idConcepto);

            ViewBag.TotalIngresos = movimientos.Sum(m => m.Ingreso);
            ViewBag.TotalGastos = movimientos.Sum(m => m.Gasto);
            ViewBag.Saldo = ViewBag.TotalIngresos - ViewBag.TotalGastos;

            return View(movimientos);
        }

        // ==========================================
        // 3. GESTIÓN DE COMUNEROS Y CONCEPTOS DINÁMICOS
        // ==========================================
        [HttpGet]
        public IActionResult Catalogos()
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            ViewBag.Comuneros = _dal.ListarComuneros(false);
            ViewBag.Conceptos1 = _dal.ListarConceptos(1);
            ViewBag.Conceptos2 = _dal.ListarConceptos(2);
            ViewBag.Conceptos3 = _dal.ListarConceptos(3);
            ViewBag.Conceptos4 = _dal.ListarConceptos(4);

            return View();
        }

        // ==========================================
        // 4. AJUSTES DEL SISTEMA Y CUENTAS
        // ==========================================
        [HttpGet]
        public IActionResult Ajustes()
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Acceso");

            ViewBag.NombreCompleto = HttpContext.Session.GetString("NombreCompleto");
            ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario") ?? "admin";

            if (!EsSoloConsulta())
            {
                var (uHerm, nHerm) = _dal.ObtenerCuentaConsulta();
                ViewBag.UsuarioHermanos = uHerm;
                ViewBag.NombreHermanos = nHerm;
            }
            return View();
        }

        [HttpPost]
        public IActionResult Ajustes(string claveActual, string nuevoUsuario, string nuevoNombre, string? nuevaClave)
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
            if (idUsuario == null) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            var (exito, mensaje) = _dal.ActualizarCredenciales(idUsuario.Value, claveActual, nuevoUsuario.Trim(), nuevoNombre.Trim(), nuevaClave?.Trim());

            if (exito)
            {
                HttpContext.Session.SetString("NombreCompleto", nuevoNombre.Trim());
                HttpContext.Session.SetString("NombreUsuario", nuevoUsuario.Trim());
                TempData["Mensaje"] = mensaje;
                return RedirectToAction("Ajustes");
            }

            ViewBag.Error = mensaje;
            ViewBag.NombreCompleto = HttpContext.Session.GetString("NombreCompleto");
            ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario") ?? "admin";
            var (uHerm, nHerm) = _dal.ObtenerCuentaConsulta();
            ViewBag.UsuarioHermanos = uHerm;
            ViewBag.NombreHermanos = nHerm;
            return View();
        }

        [HttpPost]
        public IActionResult ActualizarCuentaHermanos(string usuarioHermanos, string nombreHermanos, string? claveHermanos)
        {
            if (!EstaAutenticado()) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            var (exito, mensaje) = _dal.ActualizarCuentaConsulta(usuarioHermanos.Trim(), nombreHermanos.Trim(), claveHermanos?.Trim());
            if (exito)
                TempData["Mensaje"] = mensaje;
            else
                ViewBag.Error = mensaje;

            return RedirectToAction("Ajustes");
        }
    }
}
