using ManejoPresupuesto.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class cuenta
    {
        public int Id { get; set; }

        [StringLength(maximumLength: 50)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Nombre { get; set; }

        [StringLength(maximumLength:1000)]
        public string Descripcion { get; set; }

        [Display(Name = "Tipo cuenta")]
        public int TipoCuentaId { get; set; }
        public decimal Balance { get; set; }
        public string TipoCuenta { get; set; }
    }
}