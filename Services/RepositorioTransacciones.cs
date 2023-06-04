using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioTransacciones
    {
        Task Crear(Transaccion transaccion);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId);
        Task Borrar(int id);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesCuenta modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParamGetTransactionByUser modelo);
        Task<IEnumerable<ResultObtenerSemana>> ObtenerPorSemana(ParamGetTransactionByUser modelo);
        Task<IEnumerable<ResultadoPorMes>> ObtenerPorMes(int usuarioId, int año);
    }

    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;

        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"Transacciones_insertar", new {
                transaccion.UsuarioId,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                transaccion.CategoriaId,
                transaccion.CuentaId
            }, commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaIdAnterior)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Actualizar_Transacciones", 
            new { transaccion.Id, transaccion.FechaTransaccion, transaccion.CuentaId, cuentaIdAnterior, transaccion.CategoriaId, transaccion.Nota, transaccion.Monto, montoAnterior},
            commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"SELECT Transacciones.*, cat.TipoOperacionId FROM Transacciones INNER JOIN Categorias cat ON cat.Id = Transacciones.CategoriaId WHERE Transacciones.Id = @id AND Transacciones.UsuarioId = @usuarioId;", new {id, usuarioId});
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar", new {id}, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId FROM Transacciones t INNER JOIN Categorias c ON c.Id = t.CategoriaId INNER JOIN Cuentass cu ON cu.Id = t.CuentaId WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin;", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParamGetTransactionByUser modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, Nota, cu.Nombre as Cuenta, c.TipoOperacionId FROM Transacciones t INNER JOIN Categorias c ON c.Id = t.CategoriaId INNER JOIN Cuentass cu ON cu.Id = t.CuentaId WHERE t.UsuarioId = @UsuarioId AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin ORDER BY t.FechaTransaccion DESC;", modelo);
        }

        public async Task<IEnumerable<ResultObtenerSemana>> ObtenerPorSemana(ParamGetTransactionByUser modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultObtenerSemana>(@"SELECT datediff(d, @fechaInicio, @fechaFin) / 7 + 1 as Semana,
            SUM(Monto) as Monto, cat.TipoOperacionId
            FROM Transacciones INNER JOIN Categorias cat
            ON cat.Id = Transacciones.CategoriaId
            WHERE Transacciones.UsuarioId = @UsuarioId AND
            FechaTransaccion BETWEEN @fechaInicio and @fechaFin
            GROUP BY datediff(d, @fechaInicio, FechaTransaccion) / 7, cat.TipoOperacionId", modelo);
        }

        public async Task<IEnumerable<ResultadoPorMes>> ObtenerPorMes(int usuarioId, int año)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoPorMes>(@"SELECT MONTH(FechaTransaccion) as Mes, SUM(Monto) as Monto, cat.TipoOperacionId From Transacciones INNER JOIN Categorias cat ON cat.Id = Transacciones.CategoriaId WHERE Transacciones.UsuarioId = @usuarioId AND YEAR(FechaTransaccion) = @año GROUP BY MONTH(FechaTransaccion), cat.TipoOperacionId", new {usuarioId, año});
        }
    }
}
