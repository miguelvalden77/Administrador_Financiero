using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class CreateCuentaView : cuenta
    {
        public IEnumerable<SelectListItem> TiposCuentas {get; set;}
    }
}