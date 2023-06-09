using Microsoft.AspNetCore.Mvc;
using ManejoPresupuesto.Services;
using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IRepositorioCuenta repositorioCuenta;
        private readonly IRepositorioCategoria repositorioCategoria;
        private readonly IServicioReporte servicioReporte;

        public TransaccionesController(IServicioUsuarios servicioUsuarios, IRepositorioTransacciones repositorioTransacciones, IRepositorioCuenta repositorioCuenta, IRepositorioCategoria repositorioCategoria, IServicioReporte servicioReporte)
        {
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioTransacciones = repositorioTransacciones;
            this.repositorioCuenta = repositorioCuenta;
            this.repositorioCategoria = repositorioCategoria;
            this.servicioReporte = servicioReporte;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(int mes, int año)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var modelo = await servicioReporte.ObtenerTransaccionesEnTransacciones(usuarioId, mes, año, ViewBag);

            return View(modelo);
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var modelo = new TransaccionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionViewModel modeloView)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            if(!ModelState.IsValid)
            {
                modeloView.Cuentas = await ObtenerCuentas(usuarioId);
                modeloView.Categorias = await ObtenerCategorias(usuarioId, modeloView.TipoOperacionId);
                return View(modeloView);
            }

            var cuenta = await repositorioCuenta.GetOneCuenta(modeloView.CuentaId, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCategoria.ObtenerUnaCategoria(modeloView.CategoriaId, usuarioId);
            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modeloView.UsuarioId = usuarioId;
            if(modeloView.TipoOperacionId == TipoOperacion.Gasto)
            {
                modeloView.Monto *= -1;
            }

            await repositorioTransacciones.Crear(modeloView);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuenta.GetCuentas(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        public async Task<IActionResult> Semanal(int mes, int año)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            IEnumerable<ResultObtenerSemana> transacciones = await servicioReporte.ObtenerReporteSemanal(usuarioId, mes, año, ViewBag);

            var agrupado = transacciones.GroupBy(x=> x.Semana).Select(x => new ResultObtenerSemana()
            {
                Semana = x.Key,
                Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault()
            }).ToList();

            if(mes == 0 || año == 0)
            {
                DateTime hoy = DateTime.Now;
                mes = hoy.Month;
                año = hoy.Year;
            }

            var fechaReferencia = new DateTime(año, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);
            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for(int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(año, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if(grupoSemana is null)
                {
                    agrupado.Add(new ResultObtenerSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                } 
                else 
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }

            } 
                agrupado.OrderByDescending(x => x.Semana).ToList();
                var modelo = new ReporteSemanalViewModel();
                modelo.TransaccionesPorSemana = agrupado;
                modelo.FechaReferencia = fechaReferencia;

            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int año)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            if(año == 0)
            {
                año = DateTime.Today.Year;
            }

            var transaccionesPorMes = await repositorioTransacciones.ObtenerPorMes(usuarioId, año);
            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes).Select(x => new ResultadoPorMes()
            {
                Mes = x.Key,
                Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault()
            }).ToList();

            for(int mes = 1; mes <= 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(año, mes, 1);
                if(transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }

            }
                transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

                var modelo = new ReporteMensualViewModel();
                modelo.año = año;
                modelo.TransaccionesPorMes = transaccionesAgrupadas;

            return View(modelo);
        }

        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParamGetTransactionByUser()
            {
                UsuarioId = usuarioId,
                FechaInicio = start,
                FechaFin = end
            });

            var eventosCalendario = transacciones.Select(x => new EventoCalendario()
            {
                Title = x.Monto.ToString(),
                Start = x.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = x.FechaTransaccion.ToString("yyyy-MM-dd"),
                Color = (x.TipoOperacionId == TipoOperacion.Gasto) ? "red" : "blue"
            });

            return Json(eventosCalendario);
        }

        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParamGetTransactionByUser()
            {
                UsuarioId = usuarioId,
                FechaInicio = fecha,
                FechaFin = fecha
            });

            return Json(transacciones);
        }

        public IActionResult ExcelReporte()
        {
            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAño(int año)
        {
            var fechaInicio = new DateTime(año, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParamGetTransactionByUser()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarTodoExcel()
        {
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(100);
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParamGetTransactionByUser()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Manejo Presupuesto - Todo.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        public async Task<FileResult> ExportarExecelPorMes(int mes, int año)
        {
            var fechaInicio = new DateTime(año, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var usuarioId = servicioUsuarios.GetUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParamGetTransactionByUser()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        private FileResult GenerarExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto")
            });

            foreach(var transaccion in transacciones)
            {
                dataTable.Rows.Add(transaccion.FechaTransaccion,
                transaccion.Cuenta, transaccion.Categoria, transaccion.Nota, transaccion.Monto, transaccion.TipoOperacionId);
            }

            using(XLWorkbook wb = new XLWorkbook())
            {
                wb.AddWorksheet(dataTable);

                using(MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
                }
            }
        }

        public async Task<IActionResult> ObtenerCategorias ([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategoria.ObtenerCategoriaOperacion(usuarioId, tipoOperacion);
            var resultado = categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToList();
            var opcionPorDefecto = new SelectListItem("--- Seleccione una categoría ---", "0", true);

            resultado.Insert(0, opcionPorDefecto);

            return resultado;
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if(transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = new TransaccionActualizar
            {
                Id = transaccion.Id,
                UsuarioId = transaccion.UsuarioId,
                Monto = transaccion.Monto,
                CategoriaId = transaccion.CategoriaId,
                CuentaId = transaccion.CuentaId,
                TipoOperacionId = transaccion.TipoOperacionId,
                FechaTransaccion = transaccion.FechaTransaccion,
                Nota = transaccion.Nota
            };

            if(modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto *= -1;
            }

            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.UrlRetorno = urlRetorno;

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizar modelo)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();

            if(!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await repositorioCuenta.GetOneCuenta(modelo.CuentaId, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }    

            var categoria = await repositorioCategoria.ObtenerUnaCategoria(modelo.CategoriaId, usuarioId);
            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = new Transaccion
            {
                Id = modelo.Id,
                CategoriaId = modelo.CategoriaId,
                UsuarioId = modelo.UsuarioId,
                FechaTransaccion = modelo.FechaTransaccion,
                Nota = modelo.Nota,
                CuentaId = modelo.CuentaId,
                Monto = modelo.Monto,
                TipoOperacionId = modelo.TipoOperacionId
            };

            modelo.Monto = modelo.Monto;

            if(modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }

            await repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);

            if(string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(modelo.UrlRetorno);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if(transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTransacciones.Borrar(id);

            if(string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }
        }

    }
}