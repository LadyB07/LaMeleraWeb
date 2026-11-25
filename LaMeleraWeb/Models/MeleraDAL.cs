using Npgsql;
using NpgsqlTypes;
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
            cmd.Parameters.Add("@NombreUsuario", NpgsqlDbType.Varchar).Value = nombreUsuario;
            cmd.Parameters.Add("@Clave", NpgsqlDbType.Varchar).Value = clave;
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
            cmd.Parameters.Add("@SoloActivos", NpgsqlDbType.Boolean).Value = soloActivos;
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
            cmd.Parameters.Add("@IdApartado", NpgsqlDbType.Integer).Value = idApartado;
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
            cmd.Parameters.Add("@IdApartado", NpgsqlDbType.Integer).Value = idApartado;
            cmd.Parameters.Add("@NombreConcepto", NpgsqlDbType.Varchar).Value = nombreConcepto;
            cmd.Parameters.Add("@AdmiteLitros", NpgsqlDbType.Boolean).Value = admiteLitros;
            con.Open();
            cmd.ExecuteNonQuery();
        }

        public void GuardarComunero(string nombre, string? telefono)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT sp_GuardarComunero(@NombreCompleto, @Telefono)", con);
            cmd.Parameters.Add("@NombreCompleto", NpgsqlDbType.Varchar).Value = nombre;
            cmd.Parameters.Add("@Telefono", NpgsqlDbType.Varchar).Value = (object?)telefono ?? DBNull.Value;
            con.Open();
            cmd.ExecuteNonQuery();
        }

        public void DarBajaComunero(int idComunero, DateTime fechaBaja)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT sp_DarBajaComunero(@IdComunero, @FechaBaja)", con);
            cmd.Parameters.Add("@IdComunero", NpgsqlDbType.Integer).Value = idComunero;
            cmd.Parameters.Add("@FechaBaja", NpgsqlDbType.Date).Value = fechaBaja.Date;
            con.Open();
            cmd.ExecuteNonQuery();
        }

        // 3. INSERCIÓN DE LOS 4 MÓDULOS
        public void InsertarIngresoComunidad(IngresoComunidad m, int idUsuario)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT sp_InsertarIngresoComunidad(@Fecha, @IdComunero, @IdConcepto, @PeriodoMes, @PeriodoAnio, @Importe, @Notas, @IdUsuario)", con);
            cmd.Parameters.Add("@Fecha", NpgsqlDbType.Date).Value = m.Fecha.Date;
            cmd.Parameters.Add("@IdComunero", NpgsqlDbType.Integer).Value = m.IdComunero;
            cmd.Parameters.Add("@IdConcepto", NpgsqlDbType.Integer).Value = m.IdConcepto;
            cmd.Parameters.Add("@PeriodoMes", NpgsqlDbType.Integer).Value = (object?)m.PeriodoMes ?? DBNull.Value;
            cmd.Parameters.Add("@PeriodoAnio", NpgsqlDbType.Integer).Value = (object?)m.PeriodoAnio ?? DBNull.Value;
            cmd.Parameters.Add("@Importe", NpgsqlDbType.Numeric).Value = m.Importe;
            cmd.Parameters.Add("@Notas", NpgsqlDbType.Varchar).Value = (object?)m.Notas ?? DBNull.Value;
            cmd.Parameters.Add("@IdUsuario", NpgsqlDbType.Integer).Value = idUsuario;
            con.Open();
            cmd.ExecuteNonQuery();
        }

        public void InsertarGasto(GastoGeneral m, int idUsuario, bool esOlivar)
        {
            string sp = esOlivar ? "sp_InsertarGastoOlivar" : "sp_InsertarGastoZonaComun";
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new($"SELECT {sp}(@Fecha, @IdConcepto, @NroFacturaRecibo, @Litros, @Importe, @Notas, @RutaComprobante, @IdUsuario)", con);
            cmd.Parameters.Add("@Fecha", NpgsqlDbType.Date).Value = m.Fecha.Date;
            cmd.Parameters.Add("@IdConcepto", NpgsqlDbType.Integer).Value = m.IdConcepto;
            cmd.Parameters.Add("@NroFacturaRecibo", NpgsqlDbType.Varchar).Value = (object?)m.NroFacturaRecibo ?? DBNull.Value;
            cmd.Parameters.Add("@Litros", NpgsqlDbType.Numeric).Value = (object?)m.Litros ?? DBNull.Value;
            cmd.Parameters.Add("@Importe", NpgsqlDbType.Numeric).Value = m.Importe;
            cmd.Parameters.Add("@Notas", NpgsqlDbType.Varchar).Value = (object?)m.Notas ?? DBNull.Value;
            cmd.Parameters.Add("@RutaComprobante", NpgsqlDbType.Varchar).Value = (object?)m.RutaComprobante ?? DBNull.Value;
            cmd.Parameters.Add("@IdUsuario", NpgsqlDbType.Integer).Value = idUsuario;
            con.Open();
            cmd.ExecuteNonQuery();
        }

        public void InsertarIngresoOlivar(IngresoOlivar m, int idUsuario)
        {
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT sp_InsertarIngresoOlivar(@Fecha, @IdConcepto, @KgAceituna, @KgAceite, @PrecioPorKg, @Importe, @Notas, @IdUsuario)", con);
            cmd.Parameters.Add("@Fecha", NpgsqlDbType.Date).Value = m.Fecha.Date;
            cmd.Parameters.Add("@IdConcepto", NpgsqlDbType.Integer).Value = m.IdConcepto;
            cmd.Parameters.Add("@KgAceituna", NpgsqlDbType.Numeric).Value = (object?)m.KgAceituna ?? DBNull.Value;
            cmd.Parameters.Add("@KgAceite", NpgsqlDbType.Numeric).Value = (object?)m.KgAceite ?? DBNull.Value;
            cmd.Parameters.Add("@PrecioPorKg", NpgsqlDbType.Numeric).Value = (object?)m.PrecioPorKg ?? DBNull.Value;
            cmd.Parameters.Add("@Importe", NpgsqlDbType.Numeric).Value = m.Importe;
            cmd.Parameters.Add("@Notas", NpgsqlDbType.Varchar).Value = (object?)m.Notas ?? DBNull.Value;
            cmd.Parameters.Add("@IdUsuario", NpgsqlDbType.Integer).Value = idUsuario;
            con.Open();
            cmd.ExecuteNonQuery();
        }

        // 4. CONTROL DE DEUDAS Y REPORTES
        public List<EstadoCuotaAnual> ObtenerEstadoCuotas(int anio)
        {
            var lista = new List<EstadoCuotaAnual>();
            using NpgsqlConnection con = new(_cadenaConexion);
            using NpgsqlCommand cmd = new("SELECT * FROM sp_EstadoCuotasAnual(@Anio)", con);
            cmd.Parameters.Add("@Anio", NpgsqlDbType.Integer).Value = anio;
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
            cmd.Parameters.Add("@IdUsuario", NpgsqlDbType.Integer).Value = idUsuario;
            cmd.Parameters.Add("@ClaveActual", NpgsqlDbType.Varchar).Value = claveActual;
            cmd.Parameters.Add("@NuevoNombreUsuario", NpgsqlDbType.Varchar).Value = nuevoUsuario;
            cmd.Parameters.Add("@NuevoNombreCompleto", NpgsqlDbType.Varchar).Value = nuevoNombre;
            cmd.Parameters.Add("@NuevaClave", NpgsqlDbType.Varchar).Value = (object?)nuevaClave ?? DBNull.Value;
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
            cmd.Parameters.Add("@NuevoUsuario", NpgsqlDbType.Varchar).Value = nuevoUsuario;
            cmd.Parameters.Add("@NuevoNombre", NpgsqlDbType.Varchar).Value = nuevoNombre;
            cmd.Parameters.Add("@NuevaClave", NpgsqlDbType.Varchar).Value = (object?)nuevaClave ?? DBNull.Value;
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
            cmd.Parameters.Add("@FechaInicio", NpgsqlDbType.Date).Value = inicio.Date;
            cmd.Parameters.Add("@FechaFin", NpgsqlDbType.Date).Value = fin.Date;
            cmd.Parameters.Add("@IdApartado", NpgsqlDbType.Integer).Value = (object?)idApartado ?? DBNull.Value;
            cmd.Parameters.Add("@IdConcepto", NpgsqlDbType.Integer).Value = (object?)idConcepto ?? DBNull.Value;
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
