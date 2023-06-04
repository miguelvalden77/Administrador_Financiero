using Microsoft.AspNetCore.Mvc;
using ManejoPresupuesto.Services;
using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {

        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuenta repositorioCuenta;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioReporte servicioReporte;

        public CuentasController(IRepositorioCuentas repositorioCuentas, IServicioUsuarios servicioUsuarios, IRepositorioCuenta repositorioCuenta, IRepositorioTransacciones repositorioTransacciones, IServicioReporte servicioReporte)
        {
            this.repositorioCuentas = repositorioCuentas;
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuenta = repositorioCuenta;
            this.repositorioTransacciones = repositorioTransacciones;
            this.servicioReporte = servicioReporte;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var cuentasConTipoCuenta = await repositorioCuenta.GetCuentas(usuarioId);

            var modelo = cuentasConTipoCuenta.GroupBy(x => x.TipoCuenta).Select(grupo => new IndexCuentaViewModel{
                TipoCuenta = grupo.Key,
                Cuentas = grupo.AsEnumerable()
            }).ToList();

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            int usuarioId = servicioUsuarios.GetUsuarioId();
            var TiposCuentas = await repositorioCuentas.GetTiposCuentas(usuarioId);
            var modelo = new CreateCuentaView();
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CreateCuentaView cuenta)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var tipoCuenta = await repositorioCuentas.GetById(cuenta.TipoCuentaId, usuarioId);

            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if(!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await repositorioCuenta.CrearCuenta(cuenta);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioCuentas.GetTiposCuentas(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }  

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var cuenta = await repositorioCuenta.GetOneCuenta(id, usuarioId);
            
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = new CreateCuentaView()
            {
                Id = cuenta.Id,
                Nombre = cuenta.Nombre,
                TipoCuentaId = cuenta.TipoCuentaId,
                Descripcion = cuenta.Descripcion,
                Balance = cuenta.Balance
            };
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CreateCuentaView cuentaView)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var cuenta = await repositorioCuenta.GetOneCuenta(cuentaView.Id, usuarioId);

            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await repositorioCuentas.GetById(cuentaView.TipoCuentaId, usuarioId);

            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuenta.UpdateCuenta(cuentaView);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var cuenta = await repositorioCuenta.GetOneCuenta(id, usuarioId);

            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var cuenta = await repositorioCuenta.GetOneCuenta(id, usuarioId);

            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuenta.DeleteCuenta(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id, int mes, int año)
        {
            var usuarioId = servicioUsuarios.GetUsuarioId();
            var cuenta = await repositorioCuenta.GetOneCuenta(id, usuarioId);

            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            ViewBag.Cuenta = cuenta.Nombre;

            var modelo = await servicioReporte.ObtenerTransaccionesDetalladas(usuarioId, id, mes, año, ViewBag);

            return View(modelo);
        }
    }
}

