using Npgsql;
using System.Data;

namespace LaMeleraWeb.Models
{
    public class MeleraDAL
    {
        private readonly string _cadenaConexion;

        public MeleraDAL(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("ConexionMelera")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");
        }

        // 1. VALIDAR LOGIN
        public Usuario? ValidarUsuario(string nombreUsuario, string clave)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT * FROM sp_ValidarUsuario(@NombreUsuario, @Clave)", con);
            cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
            cmd.Parameters.AddWithValue("@Clave", clave);
            con.Open();

            using NpgsqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return new Usuario
                {
                    IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                    NombreCompleto = dr["NombreCompleto"].ToString()!,
                    Rol = dr["Rol"].ToString()!
                };
            }
            return null;
        }

        // 2. CATÁLOGOS Y COMUNEROS
        public List<Comunero> ListarComuneros(bool soloActivos = true)
        {
            var lista = new List<Comunero>();
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT * FROM sp_ListarComuneros(@SoloActivos)", con);
            cmd.Parameters.AddWithValue("@SoloActivos", soloActivos);
            con.Open();

            using NpgsqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Comunero
                {
                    IdComunero = Convert.ToInt32(dr["IdComunero"]),
                    NombreCompleto = dr["NombreCompleto"].ToString()!,
                    Telefono = dr["Telefono"] == DBNull.Value ? null : dr["Telefono"].ToString(),
                    Activo = Convert.ToBoolean(dr["Activo"]),
                    FechaAlta = Convert.ToDateTime(dr["FechaAlta"]),
                    FechaBaja = dr["FechaBaja"] == DBNull.Value ? null : Convert.ToDateTime(dr["FechaBaja"])
                });
            }
            return lista;
        }

        public List<Concepto> ListarConceptos(int idApartado)
        {
            var lista = new List<Concepto>();
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT * FROM sp_ListarConceptosPorApartado(@IdApartado)", con);
            cmd.Parameters.AddWithValue("@IdApartado", idApartado);
            con.Open();

            using NpgsqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Concepto
                {
                    IdConcepto = Convert.ToInt32(dr["IdConcepto"]),
                    IdApartado = idApartado,
                    NombreConcepto = dr["NombreConcepto"].ToString()!,
                    AdmiteLitros = Convert.ToBoolean(dr["AdmiteLitros"])
                });
            }
            return lista;
        }

        public void CrearConcepto(int idApartado, string nombreConcepto, bool admiteLitros)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT sp_CrearConcepto(@IdApartado, @NombreConcepto, @AdmiteLitros)", con);
            cmd.Parameters.AddWithValue("@IdApartado", idApartado);
            cmd.Parameters.AddWithValue("@NombreConcepto", nombreConcepto);
            cmd.Parameters.AddWithValue("@AdmiteLitros", admiteLitros);
            con.Open();
            cmd.ExecuteNonQuery();
        }

        public void GuardarComunero(string nombre, string? telefono)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT sp_GuardarComunero(@NombreCompleto, @Telefono)", con);
            cmd.Parameters.AddWithValue("@NombreCompleto", nombre);
            cmd.Parameters.AddWithValue("@Telefono", (object?)telefono ?? DBNull.Value);
            con.Open();
            cmd.ExecuteNonQuery();
        }

        public void DarBajaComunero(int idComunero, DateTime fechaBaja)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT sp_DarBajaComunero(@IdComunero, @FechaBaja)", con);
            cmd.Parameters.AddWithValue("@IdComunero", idComunero);
            cmd.Parameters.AddWithValue("@FechaBaja", fechaBaja);
            con.Open();
            cmd.ExecuteNonQuery();
        }

        // 3. INSERCIÓN DE LOS 4 MÓDULOS
        public void InsertarIngresoComunidad(IngresoComunidad m, int idUsuario)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT sp_InsertarIngresoComunidad(@Fecha, @IdComunero, @IdConcepto, @PeriodoMes, @PeriodoAnio, @Importe, @Notas, @IdUsuario)", con);
            cmd.Parameters.AddWithValue("@Fecha", m.Fecha);
            cmd.Parameters.AddWithValue("@IdComunero", m.IdComunero);
            cmd.Parameters.AddWithValue("@IdConcepto", m.IdConcepto);
            cmd.Parameters.AddWithValue("@PeriodoMes", (object?)m.PeriodoMes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PeriodoAnio", (object?)m.PeriodoAnio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Importe", m.Importe);
            cmd.Parameters.AddWithValue("@Notas", (object?)m.Notas ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            con.Open();
            cmd.ExecuteNonQuery();
        }

        public void InsertarGasto(GastoGeneral m, int idUsuario, bool esOlivar)
        {
            string sp = esOlivar ? "sp_InsertarGastoOlivar" : "sp_InsertarGastoZonaComun";
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new($"SELECT {sp}(@Fecha, @IdConcepto, @NroFacturaRecibo, @Litros, @Importe, @Notas, @RutaComprobante, @IdUsuario)", con);
            cmd.Parameters.AddWithValue("@Fecha", m.Fecha);
            cmd.Parameters.AddWithValue("@IdConcepto", m.IdConcepto);
            cmd.Parameters.AddWithValue("@NroFacturaRecibo", (object?)m.NroFacturaRecibo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Litros", (object?)m.Litros ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Importe", m.Importe);
            cmd.Parameters.AddWithValue("@Notas", (object?)m.Notas ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RutaComprobante", (object?)m.RutaComprobante ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            con.Open();
            cmd.ExecuteNonQuery();
        }

        public void InsertarIngresoOlivar(IngresoOlivar m, int idUsuario)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT sp_InsertarIngresoOlivar(@Fecha, @IdConcepto, @KgAceituna, @KgAceite, @PrecioPorKg, @Importe, @Notas, @IdUsuario)", con);
            cmd.Parameters.AddWithValue("@Fecha", m.Fecha);
            cmd.Parameters.AddWithValue("@IdConcepto", m.IdConcepto);
            cmd.Parameters.AddWithValue("@KgAceituna", (object?)m.KgAceituna ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@KgAceite", (object?)m.KgAceite ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PrecioPorKg", (object?)m.PrecioPorKg ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Importe", m.Importe);
            cmd.Parameters.AddWithValue("@Notas", (object?)m.Notas ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            con.Open();
            cmd.ExecuteNonQuery();
        }

        // 4. CONTROL DE DEUDAS Y REPORTES
        public List<EstadoCuotaAnual> ObtenerEstadoCuotas(int anio)
        {
            var lista = new List<EstadoCuotaAnual>();
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT * FROM sp_EstadoCuotasAnual(@Anio)", con);
            cmd.Parameters.AddWithValue("@Anio", anio);
            con.Open();

            using NpgsqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new EstadoCuotaAnual
                {
                    IdComunero = Convert.ToInt32(dr["IdComunero"]),
                    NombreCompleto = dr["NombreCompleto"].ToString()!,
                    Ene = Convert.ToDecimal(dr["Ene"]),
                    Feb = Convert.ToDecimal(dr["Feb"]),
                    Mar = Convert.ToDecimal(dr["Mar"]),
                    Abr = Convert.ToDecimal(dr["Abr"]),
                    May = Convert.ToDecimal(dr["May"]),
                    Jun = Convert.ToDecimal(dr["Jun"]),
                    Jul = Convert.ToDecimal(dr["Jul"]),
                    Ago = Convert.ToDecimal(dr["Ago"]),
                    Sep = Convert.ToDecimal(dr["Sep"]),
                    Oct = Convert.ToDecimal(dr["Oct"]),
                    Nov = Convert.ToDecimal(dr["Nov"]),
                    Dic = Convert.ToDecimal(dr["Dic"]),
                    TotalPagado = Convert.ToDecimal(dr["TotalPagado"])
                });
            }
            return lista;
        }

        // 5. ACTUALIZAR CREDENCIALES DEL ADMINISTRADOR (AJUSTES)
        public (bool exito, string mensaje) ActualizarCredenciales(int idUsuario, string claveActual, string nuevoUsuario, string nuevoNombre, string? nuevaClave)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT * FROM sp_ActualizarCredenciales(@IdUsuario, @ClaveActual, @NuevoNombreUsuario, @NuevoNombreCompleto, @NuevaClave)", con);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            cmd.Parameters.AddWithValue("@ClaveActual", claveActual);
            cmd.Parameters.AddWithValue("@NuevoNombreUsuario", nuevoUsuario);
            cmd.Parameters.AddWithValue("@NuevoNombreCompleto", nuevoNombre);
            cmd.Parameters.AddWithValue("@NuevaClave", (object?)nuevaClave ?? DBNull.Value);
            con.Open();

            using NpgsqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return (Convert.ToInt32(dr["Resultado"]) == 1, dr["Mensaje"].ToString()!);
            }
            return (false, "Error inesperado al actualizar.");
        }

        // 6. GESTIONAR CUENTA DE HERMANOS (ROL CONSULTA)
        public (string usuario, string nombre) ObtenerCuentaConsulta()
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT NombreUsuario, NombreCompleto FROM Usuarios WHERE Rol = 'Consulta' LIMIT 1", con);
            con.Open();

            using NpgsqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return (dr["NombreUsuario"].ToString()!, dr["NombreCompleto"].ToString()!);
            }
            return ("Hermanos", "Hermanos Comuneros");
        }

        public (bool exito, string mensaje) ActualizarCuentaConsulta(string nuevoUsuario, string nuevoNombre, string? nuevaClave)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT * FROM sp_ActualizarCuentaConsulta(@NuevoUsuario, @NuevoNombre, @NuevaClave)", con);
            cmd.Parameters.AddWithValue("@NuevoUsuario", nuevoUsuario);
            cmd.Parameters.AddWithValue("@NuevoNombre", nuevoNombre);
            cmd.Parameters.AddWithValue("@NuevaClave", (object?)nuevaClave ?? DBNull.Value);
            con.Open();

            using NpgsqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return (Convert.ToInt32(dr["Resultado"]) == 1, dr["Mensaje"].ToString()!);
            }
            return (false, "Error inesperado al actualizar la cuenta de hermanos.");
        }

        // 7. REPORTE LIBRO DIARIO
        public List<MovimientoReporte> ObtenerReporte(DateTime inicio, DateTime fin, int? idApartado, int? idConcepto)
        {
            var lista = new List<MovimientoReporte>();
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT * FROM sp_ReporteMovimientos(@FechaInicio, @FechaFin, @IdApartado, @IdConcepto)", con);
            cmd.Parameters.AddWithValue("@FechaInicio", inicio);
            cmd.Parameters.AddWithValue("@FechaFin", fin);
            cmd.Parameters.AddWithValue("@IdApartado", (object?)idApartado ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdConcepto", (object?)idConcepto ?? DBNull.Value);
            con.Open();

            using NpgsqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new MovimientoReporte
                {
                    IdRegistro = Convert.ToInt32(dr["IdRegistro"]),
                    IdApartado = Convert.ToInt32(dr["IdApartado"]),
                    Fecha = Convert.ToDateTime(dr["Fecha"]),
                    Apartado = dr["Apartado"].ToString()!,
                    Concepto = dr["Concepto"].ToString()!,
                    DetalleExtra = dr["DetalleExtra"] == DBNull.Value ? null : dr["DetalleExtra"].ToString(),
                    Factura = dr["Factura"] == DBNull.Value ? null : dr["Factura"].ToString(),
                    Litros = dr["Litros"] == DBNull.Value ? null : Convert.ToDecimal(dr["Litros"]),
                    Ingreso = Convert.ToDecimal(dr["Ingreso"]),
                    Gasto = Convert.ToDecimal(dr["Gasto"]),
                    Notas = dr["Notas"] == DBNull.Value ? null : dr["Notas"].ToString(),
                    RutaComprobante = dr["RutaComprobante"] == DBNull.Value ? null : dr["RutaComprobante"].ToString()
                });
            }
            return lista;
        }

        // 8. OBTENER Y EDITAR UN MOVIMIENTO EXISTENTE
        public MovimientoEdicion? ObtenerMovimientoPorId(int idApartado, int idRegistro)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT * FROM sp_ObtenerMovimientoPorId(@IdApartado, @IdRegistro)", con);
            cmd.Parameters.AddWithValue("@IdApartado", idApartado);
            cmd.Parameters.AddWithValue("@IdRegistro", idRegistro);
            con.Open();

            using NpgsqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return new MovimientoEdicion
                {
                    IdRegistro = Convert.ToInt32(dr["IdRegistro"]),
                    IdApartado = Convert.ToInt32(dr["IdApartado"]),
                    Fecha = Convert.ToDateTime(dr["Fecha"]),
                    IdComunero = dr["IdComunero"] == DBNull.Value ? null : Convert.ToInt32(dr["IdComunero"]),
                    IdConcepto = Convert.ToInt32(dr["IdConcepto"]),
                    PeriodoMes = dr["PeriodoMes"] == DBNull.Value ? null : Convert.ToInt32(dr["PeriodoMes"]),
                    PeriodoAnio = dr["PeriodoAnio"] == DBNull.Value ? null : Convert.ToInt32(dr["PeriodoAnio"]),
                    NroFacturaRecibo = dr["NroFacturaRecibo"] == DBNull.Value ? null : dr["NroFacturaRecibo"].ToString(),
                    Litros = dr["Litros"] == DBNull.Value ? null : Convert.ToDecimal(dr["Litros"]),
                    KgAceituna = dr["KgAceituna"] == DBNull.Value ? null : Convert.ToDecimal(dr["KgAceituna"]),
                    KgAceite = dr["KgAceite"] == DBNull.Value ? null : Convert.ToDecimal(dr["KgAceite"]),
                    PrecioPorKg = dr["PrecioPorKg"] == DBNull.Value ? null : Convert.ToDecimal(dr["PrecioPorKg"]),
                    Importe = Convert.ToDecimal(dr["Importe"]),
                    Notas = dr["Notas"] == DBNull.Value ? null : dr["Notas"].ToString()
                };
            }
            return null;
        }

        public void EditarMovimiento(MovimientoEdicion m)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT sp_EditarMovimiento(@IdApartado, @IdRegistro, @Fecha, @IdComunero, @IdConcepto, @PeriodoMes, @PeriodoAnio, @NroFacturaRecibo, @Litros, @KgAceituna, @KgAceite, @PrecioPorKg, @Importe, @Notas)", con);
            cmd.Parameters.AddWithValue("@IdApartado", m.IdApartado);
            cmd.Parameters.AddWithValue("@IdRegistro", m.IdRegistro);
            cmd.Parameters.AddWithValue("@Fecha", m.Fecha);
            cmd.Parameters.AddWithValue("@IdComunero", (object?)m.IdComunero ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdConcepto", m.IdConcepto);
            cmd.Parameters.AddWithValue("@PeriodoMes", (object?)m.PeriodoMes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PeriodoAnio", (object?)m.PeriodoAnio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NroFacturaRecibo", (object?)m.NroFacturaRecibo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Litros", (object?)m.Litros ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@KgAceituna", (object?)m.KgAceituna ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@KgAceite", (object?)m.KgAceite ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PrecioPorKg", (object?)m.PrecioPorKg ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Importe", m.Importe);
            cmd.Parameters.AddWithValue("@Notas", (object?)m.Notas ?? DBNull.Value);
            con.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
