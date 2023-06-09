using Microsoft.AspNetCore.Mvc;
using ManejoPresupuesto.Services;
using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{
    public class CategoriaController : Controller
    {

        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuenta repositorioCuenta;
        private readonly IRepositorioCategoria repositorioCategoria;

        public CategoriaController(IRepositorioCategoria repositorioCategoria, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioCategoria = repositorioCategoria;
            this.servicioUsuarios = servicioUsuarios;
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if(!ModelState.IsValid)
            {
                return View(categoria);
            }

            var usuarioId = servicioUsuarios.GetUsuarioId();
            categoria.UsuarioId = usuarioId;

            await repositorioCategoria.CrearCategoria(categoria);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            int usuarioId = servicioUsuarios.GetUsuarioId();
            var categorias = await repositorioCategoria.ObtenerCategorias(usuarioId, paginacion);
            var totalCategorias = await repositorioCategoria.Contar(usuarioId);

            var respuestaVM = new PaginacionRespuesta<Categoria>
            {
                Elementos = categorias,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalDeRecords = totalCategorias,
                BaseURL = Url.Action()
            };

            return View(respuestaVM);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            int usuarioId = servicioUsuarios.GetUsuarioId();
            var categoria = await repositorioCategoria.ObtenerUnaCategoria(id, usuarioId);

            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoria)
        {
            int usuarioId = servicioUsuarios.GetUsuarioId();
            var categoriaPrueba = await repositorioCategoria.ObtenerUnaCategoria(categoria.Id, usuarioId);

            if(categoriaPrueba is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCategoria.EditarCategoria(categoria);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            int usuarioId = servicioUsuarios.GetUsuarioId();
            var categoria = await repositorioCategoria.ObtenerUnaCategoria(id, usuarioId);
            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            int usuarioId = servicioUsuarios.GetUsuarioId();
            var categoria = await repositorioCategoria.ObtenerUnaCategoria(id, usuarioId);

            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCategoria.BorrarCategoria(id);
            return RedirectToAction("Index");
        }

    }
}