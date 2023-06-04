namespace ManejoPresupuesto.Models
{
    public class ReporteMensualViewModel
    {
        public IEnumerable<ResultadoPorMes> TransaccionesPorMes {get; set;}
        public decimal Ingresos => TransaccionesPorMes.Sum(x => x.Ingreso);
        public decimal Gastos => TransaccionesPorMes.Sum(x => x.Gasto);
        public decimal Total => Ingresos - Gastos;
        public int año {get; set;}
    }
}