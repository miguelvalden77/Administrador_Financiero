using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Services
{
    public interface IServicioReporte
    {
        Task<ReporteTransacciones> ObtenerTransaccionesDetalladas(int usuarioId, int cuentaId, int mes, int año, dynamic ViewBag);
        Task<ReporteTransacciones> ObtenerTransaccionesEnTransacciones(int usuarioId, int mes, int año, dynamic ViewBag);
        Task<IEnumerable<ResultObtenerSemana>> ObtenerReporteSemanal(int usuarioId, int mes, int año, dynamic ViewBag);
    }

    public class ServicioReporte : IServicioReporte
    {

        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly HttpContext httpContext;

        public ServicioReporte(IRepositorioTransacciones repositorioTransacciones, IHttpContextAccessor httpContextAccessor)
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<ReporteTransacciones> ObtenerTransaccionesEnTransacciones(int usuarioId, int mes, int año, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioFin(mes, año);

            var parametro = new ParamGetTransactionByUser()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(parametro);
            var modelo = GeneraReporteTransacciones(fechaInicio, fechaFin, transacciones);
            AsignarViewBag(ViewBag, fechaInicio);

            return modelo;
        }

        public async Task<ReporteTransacciones> ObtenerTransaccionesDetalladas(int usuarioId, int cuentaId, int mes, int año, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioFin(mes, año);

            var obtenerTransaccionesCuenta = new ObtenerTransaccionesCuenta()
            {
                CuentaId = cuentaId,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await repositorioTransacciones.ObtenerPorCuentaId(obtenerTransaccionesCuenta);
            var modelo = GeneraReporteTransacciones(fechaInicio, fechaFin, transacciones);
            AsignarViewBag(ViewBag, fechaInicio);

            return modelo;
        }

        private void AsignarViewBag(dynamic ViewBag, DateTime fechaInicio)
        {
            ViewBag.mesAnterior = fechaInicio.AddMonths(1).Month;
            ViewBag.mesPosterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.añoPosterior = fechaInicio.AddMonths(1).Year;
            ViewBag.añoAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.urlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;
        }

        private static ReporteTransacciones GeneraReporteTransacciones(DateTime fechaInicio, DateTime fechaFin, IEnumerable<Transaccion> transacciones)
        {
            var modelo = new ReporteTransacciones();

            var transaccionesFecha = transacciones.OrderByDescending(x => x.FechaTransaccion).GroupBy(x => x.FechaTransaccion).Select(grupo => new ReporteTransacciones.TransaccionesPorFecha()
            {
                FechaTransaccion = grupo.Key,
                Transacciones = grupo.AsEnumerable()
            });

            modelo.TransaccionesAgrupadas = transaccionesFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;
            return modelo;
        }

        private (DateTime fechaInicio, DateTime fechaFin) GenerarFechaInicioFin(int mes, int año)
        {
            DateTime fechaInicio;
            DateTime fechaFin;

            if(mes <= 0 || mes > 12 || año < 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(año, mes, 1);
            }

            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return (fechaInicio, fechaFin);
        }

        public async Task<IEnumerable<ResultObtenerSemana>> ObtenerReporteSemanal(int usuarioId, int mes, int año, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioFin(mes, año);

            var parametro = new ParamGetTransactionByUser()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            AsignarViewBag(ViewBag, fechaInicio);
            var modelo = await repositorioTransacciones.ObtenerPorSemana(parametro);
            return modelo;
        }
    }
}