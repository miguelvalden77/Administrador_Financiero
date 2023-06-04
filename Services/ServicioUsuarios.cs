using System.Security.Claims;

namespace ManejoPresupuesto.Services
{
    public interface IServicioUsuarios
    {
        public int GetUsuarioId();
    }
    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly HttpContext httpContext;
        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        public int GetUsuarioId()
        {
            if(httpContext.User.Identity.IsAuthenticated)
            {
                var idClaims = httpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                var id = int.Parse(idClaims.Value);

                return id;
            }
            else
            {
                throw new ApplicationException("El usuario no está autenticado");
            }
        }
    }
}
