using ManejoPresupuesto.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class IndexCuentaViewModel
    {
        public string TipoCuenta { get; set; }
        public IEnumerable<cuenta> Cuentas { get; set; }
        public decimal Balance => Cuentas.Sum(x => x.Balance); 
    }
}