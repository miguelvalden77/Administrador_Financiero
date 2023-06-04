using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class ParamGetTransactionByUser
    {
        public int UsuarioId {get; set;}
        public DateTime FechaInicio {get; set;}
        public DateTime FechaFin {get; set;}
    }
}