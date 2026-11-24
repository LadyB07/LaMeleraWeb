namespace LaMeleraWeb.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    public class Comunero
    {
        public int IdComunero { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }
    }

    public class Concepto
    {
        public int IdConcepto { get; set; }
        public int IdApartado { get; set; }
        public string NombreConcepto { get; set; } = string.Empty;
        public bool AdmiteLitros { get; set; }
    }

    public class IngresoComunidad
    {
        public DateTime Fecha { get; set; } = DateTime.Today;
        public int IdComunero { get; set; }
        public int IdConcepto { get; set; }
        public int? PeriodoMes { get; set; }
        public int? PeriodoAnio { get; set; } = DateTime.Today.Year;
        public decimal Importe { get; set; }
        public string? Notas { get; set; }
    }

    public class GastoGeneral
    {
        public DateTime Fecha { get; set; } = DateTime.Today;
        public int IdConcepto { get; set; }
        public string? NroFacturaRecibo { get; set; }
        public decimal? Litros { get; set; }
        public decimal Importe { get; set; }
        public string? Notas { get; set; }
        public string? RutaComprobante { get; set; }
    }

    public class IngresoOlivar
    {
        public DateTime Fecha { get; set; } = DateTime.Today;
        public int IdConcepto { get; set; }
        public decimal? KgAceituna { get; set; }
        public decimal? KgAceite { get; set; }
        public decimal? PrecioPorKg { get; set; }
        public decimal Importe { get; set; }
        public string? Notas { get; set; }
    }

    public class EstadoCuotaAnual
    {
        public int IdComunero { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public decimal Ene { get; set; }
        public decimal Feb { get; set; }
        public decimal Mar { get; set; }
        public decimal Abr { get; set; }
        public decimal May { get; set; }
        public decimal Jun { get; set; }
        public decimal Jul { get; set; }
        public decimal Ago { get; set; }
        public decimal Sep { get; set; }
        public decimal Oct { get; set; }
        public decimal Nov { get; set; }
        public decimal Dic { get; set; }
        public decimal TotalPagado { get; set; }
    }

    public class MovimientoReporte
    {
        public int IdRegistro { get; set; }
        public int IdApartado { get; set; }
        public DateTime Fecha { get; set; }
        public string Apartado { get; set; } = string.Empty;
        public string Concepto { get; set; } = string.Empty;
        public string? DetalleExtra { get; set; }
        public string? Factura { get; set; }
        public decimal? Litros { get; set; }
        public decimal Ingreso { get; set; }
        public decimal Gasto { get; set; }
        public string? Notas { get; set; }
        public string? RutaComprobante { get; set; }
    }

    public class MovimientoEdicion
    {
        public int IdRegistro { get; set; }
        public int IdApartado { get; set; }
        public DateTime Fecha { get; set; }
        public int? IdComunero { get; set; }
        public int IdConcepto { get; set; }
        public int? PeriodoMes { get; set; }
        public int? PeriodoAnio { get; set; }
        public string? NroFacturaRecibo { get; set; }
        public decimal? Litros { get; set; }
        public decimal? KgAceituna { get; set; }
        public decimal? KgAceite { get; set; }
        public decimal? PrecioPorKg { get; set; }
        public decimal Importe { get; set; }
        public string? Notas { get; set; }
    }
}