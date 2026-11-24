using LaMeleraWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaMeleraWeb.Controllers
{
    public class MovimientosController : Controller
    {
        private readonly MeleraDAL _dal;
        private readonly IWebHostEnvironment _env;

        public MovimientosController(MeleraDAL dal, IWebHostEnvironment env)
        {
            _dal = dal;
            _env = env;
        }

        private int? ObtenerUsuarioSesion() => HttpContext.Session.GetInt32("IdUsuario");
        private bool EsSoloConsulta() => HttpContext.Session.GetString("Rol") == "Consulta";

        // ==========================================
        // 1. INGRESOS COMUNIDAD (Apartado 1)
        // ==========================================
        [HttpGet]
        public IActionResult IngresosComunidad()
        {
            if (ObtenerUsuarioSesion() == null) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            ViewBag.Comuneros = _dal.ListarComuneros(true);
            ViewBag.Conceptos = _dal.ListarConceptos(1);
            return View(new IngresoComunidad { Importe = 80.00m, PeriodoMes = DateTime.Today.Month });
        }

        [HttpPost]
        public IActionResult IngresosComunidad(IngresoComunidad modelo)
        {
            int? idUsuario = ObtenerUsuarioSesion();
            if (idUsuario == null) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            _dal.InsertarIngresoComunidad(modelo, idUsuario.Value);
            TempData["Mensaje"] = "Ingreso de comunidad registrado correctamente.";
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 2. GASTOS ZONAS COMUNES (Apartado 2)
        // ==========================================
        [HttpGet]
        public IActionResult GastosComunes()
        {
            if (ObtenerUsuarioSesion() == null) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            ViewBag.Conceptos = _dal.ListarConceptos(2);
            ViewBag.Titulo = "Registrar Gasto - Zonas Comunes";
            ViewBag.EsOlivar = false;
            return View("FormularioGasto", new GastoGeneral());
        }

        [HttpPost]
        public async Task<IActionResult> GastosComunes(GastoGeneral modelo, IFormFile? archivoComprobante)
        {
            int? idUsuario = ObtenerUsuarioSesion();
            if (idUsuario == null) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            modelo.RutaComprobante = await GuardarArchivoComprobante(archivoComprobante);
            _dal.InsertarGasto(modelo, idUsuario.Value, false);
            TempData["Mensaje"] = "Gasto de zonas comunes registrado correctamente.";
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 3. INGRESOS OLIVAR (Apartado 3)
        // ==========================================
        [HttpGet]
        public IActionResult IngresosOlivar()
        {
            if (ObtenerUsuarioSesion() == null) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            ViewBag.Conceptos = _dal.ListarConceptos(3);
            return View(new IngresoOlivar());
        }

        [HttpPost]
        public IActionResult IngresosOlivar(IngresoOlivar modelo)
        {
            int? idUsuario = ObtenerUsuarioSesion();
            if (idUsuario == null) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            _dal.InsertarIngresoOlivar(modelo, idUsuario.Value);
            TempData["Mensaje"] = "Ingreso de olivar registrado correctamente.";
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 4. GASTOS OLIVAR (Apartado 4)
        // ==========================================
        [HttpGet]
        public IActionResult GastosOlivar()
        {
            if (ObtenerUsuarioSesion() == null) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            ViewBag.Conceptos = _dal.ListarConceptos(4);
            ViewBag.Titulo = "Registrar Gasto - Olivar";
            ViewBag.EsOlivar = true;
            return View("FormularioGasto", new GastoGeneral());
        }

        [HttpPost]
        public async Task<IActionResult> GastosOlivar(GastoGeneral modelo, IFormFile? archivoComprobante)
        {
            int? idUsuario = ObtenerUsuarioSesion();
            if (idUsuario == null) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            modelo.RutaComprobante = await GuardarArchivoComprobante(archivoComprobante);
            _dal.InsertarGasto(modelo, idUsuario.Value, true);
            TempData["Mensaje"] = "Gasto de olivar registrado correctamente.";
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 5. EDITAR MOVIMIENTO EXISTENTE (SOLO ADMIN)
        // ==========================================
        [HttpGet]
        public IActionResult Editar(int idApartado, int idRegistro)
        {
            if (ObtenerUsuarioSesion() == null) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            var mov = _dal.ObtenerMovimientoPorId(idApartado, idRegistro);
            if (mov == null) return RedirectToAction("Index", "Home");

            ViewBag.Conceptos = _dal.ListarConceptos(idApartado);
            if (idApartado == 1)
                ViewBag.Comuneros = _dal.ListarComuneros(false);

            return View("EditarMovimiento", mov);
        }

        [HttpPost]
        public IActionResult Editar(MovimientoEdicion modelo)
        {
            if (ObtenerUsuarioSesion() == null) return RedirectToAction("Login", "Acceso");
            if (EsSoloConsulta()) return RedirectToAction("Index", "Home");

            _dal.EditarMovimiento(modelo);
            TempData["Mensaje"] = "Movimiento actualizado correctamente.";
            return RedirectToAction("Reportes", "Gestion");
        }

        // ==========================================
        // AUXILIAR: GUARDAR COMPROBANTE (PDF / IMAGEN)
        // ==========================================
        private async Task<string?> GuardarArchivoComprobante(IFormFile? archivo)
        {
            if (archivo == null || archivo.Length == 0) return null;

            string ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            string[] permitidas = { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
            if (!permitidas.Contains(ext)) return null;

            string carpeta = Path.Combine(_env.WebRootPath, "comprobantes");
            Directory.CreateDirectory(carpeta);

            string nombreUnico = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..6]}{ext}";
            string rutaFisica = Path.Combine(carpeta, nombreUnico);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return "/comprobantes/" + nombreUnico;
        }
    }
}