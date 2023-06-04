using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioCuenta
    {
        Task CrearCuenta(cuenta cuenta);
        Task<IEnumerable<cuenta>> GetCuentas(int usuarioId);
        Task<CreateCuentaView> GetOneCuenta(int id, int usuarioId);
        Task UpdateCuenta(CreateCuentaView cuenta);
        Task DeleteCuenta(int id);
    }

    public class RepositorioCuenta : IRepositorioCuenta
    {
        private readonly string connectionString;

        public RepositorioCuenta(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConection");
        }

        public async Task CrearCuenta(cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Cuentass (Nombre, TipoCuentaId, Balance, Descripcion) 
            VALUES (@Nombre, @TipoCuentaId, @Balance, @Descripcion); SELECT SCOPE_IDENTITY();", cuenta);
            cuenta.Id = id;
        }

        public async Task<IEnumerable<cuenta>> GetCuentas(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var cuentas = await connection.QueryAsync<cuenta>(@"SELECT cu.Id, cu.Nombre, cu.Balance, cu.Descripcion, tc.Nombre as TipoCuenta FROM Cuentass cu INNER JOIN TiposCuentas tc ON tc.Id = cu.TipoCuentaId
            WHERE UsuarioId = @usuarioId ORDER BY tc.Orden", new {usuarioId});

            return cuentas;
        }

        public async Task<CreateCuentaView> GetOneCuenta(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var cuenta = await connection.QueryFirstOrDefaultAsync<CreateCuentaView>(@"SELECT Cuentass.Id, Cuentass.Nombre, Balance, Descripcion, TipoCuentaId FROM Cuentass INNER JOIN TiposCuentas tc ON tc.Id = Cuentass.TipoCuentaId WHERE tc.UsuarioId = @usuarioId AND Cuentass.Id = @id;", new {id, usuarioId});

            return cuenta;
        }

        public async Task UpdateCuenta(CreateCuentaView cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Cuentass SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion, TipoCuentaId = @TipoCuentaId WHERE Id = @Id", cuenta);
        }

        public async Task DeleteCuenta(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Cuentass WHERE Id = @Id;", new {id});
        }
    }
}
