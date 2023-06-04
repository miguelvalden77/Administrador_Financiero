using Microsoft.AspNetCore.Mvc;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using Dapper;
using ManejoPresupuesto.Services;

namespace ManejoPresupuesto.Controllers
{

    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;
        public TiposCuentasController(IRepositorioCuentas repositorioCuentas, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioTiposCuentas = repositorioCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }

        public IActionResult Crear() 
        { 
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var UsuarioId = servicioUsuarios.GetUsuarioId();
            var TiposCuentas = await repositorioTiposCuentas.GetTiposCuentas(UsuarioId);

            return View(TiposCuentas);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {

            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            tipoCuenta.UsuarioId = servicioUsuarios.GetUsuarioId();

            var yaExiste = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);
            if (yaExiste)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe");
                return View(tipoCuenta);
            }

            await repositorioTiposCuentas.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> VerificarExiste(string nombre)
        {
            int UsuarioId = servicioUsuarios.GetUsuarioId();
            bool YaExiste = await repositorioTiposCuentas.Existe(nombre, UsuarioId);

            if (YaExiste)
            {
                return Json($"El nombre {nombre} ya existe en la BBDD");
            }

            return Json(true);
        }

        public async Task<IActionResult> Update(int id)
        {
            int UsuarioId = servicioUsuarios.GetUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.GetById(id, UsuarioId);
            if (tipoCuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Update(TipoCuenta tipoCuenta)
        {
            int UsuarioId = servicioUsuarios.GetUsuarioId();
            var tipoCuentaExiste = repositorioTiposCuentas.Existe(tipoCuenta.Nombre, UsuarioId);
            if(tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.UpdateTipoCuenta(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTipoCuenta(int id)
        {
            int usuarioId = servicioUsuarios.GetUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.GetById(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Delete(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            int usuarioId = servicioUsuarios.GetUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.GetById(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Order([FromBody] int[] ids) 
        {
            int usuarioId = servicioUsuarios.GetUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.GetTiposCuentas(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentasNoIncluidos = ids.Except(idsTiposCuentas).ToList();

            if (idsTiposCuentasNoIncluidos.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenados = ids.Select((valor, index) => new TipoCuenta() { Id = valor, Orden = index + 1}).AsEnumerable();
            await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);

            return Ok();
        }
    }

}

