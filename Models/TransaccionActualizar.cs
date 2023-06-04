using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class TransaccionActualizar : TransaccionViewModel
    {
       public int CuentaAnteriorId {get; set;}
       public decimal MontoAnterior {get; set;}
       public string UrlRetorno {get; set;}
    }
}