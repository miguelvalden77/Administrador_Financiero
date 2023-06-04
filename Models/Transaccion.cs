using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Transaccion
    {
        public int Id {get; set;}
        public int UsuarioId {get; set;}
        [Display(Name = "Fecha Transacción")]
        [DataType(DataType.Date)]
        public DateTime FechaTransaccion {get; set;} = DateTime.Now;
        public decimal Monto {get; set;}
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debes seleccionar una categoría")]
        [Display(Name = "Categoría")]
        public int CategoriaId {get; set;}
        [StringLength(maximumLength: 1000, ErrorMessage = "La nota no debe sobrepasar {1} caracteres")]
        public string Nota {get; set;}
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debes seleccionar una cuenta")]
        [Display(Name = "Tipo de cuenta")]
        public int CuentaId {get; set;}
        [Display(Name = "Tipo de operación")]
        public TipoOperacion TipoOperacionId {get; set;} = TipoOperacion.Ingreso;
        public string Cuenta {get; set;}
        public string Categoria {get; set;}
    }
}
