using ManejoPresupuesto.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class TipoCuenta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "La longitud debe ser de m�nimo {2} y m�ximo {1}")]
        [Display(Name = "Nombre del tipo de cuenta")]
        [FirstLetterToUpper]
        [Remote(action: "VerificarExiste", controller:"TiposCuentas", AdditionalFields = nameof(Id))]
        public string Nombre {get; set;}
        public int UsuarioId {get; set;}
        public int Orden {get; set;}
    }
}
