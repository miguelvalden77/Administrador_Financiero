using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioCuentas
    {
        Task Crear(TipoCuenta tipoCuenta);
        Task Delete(int id);
        Task<bool> Existe(string Nombre, int UsuarioId);
        Task<TipoCuenta> GetById(int Id, int UsuarioId);
        Task<IEnumerable<TipoCuenta>> GetTiposCuentas(int UsuarioID);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
        Task UpdateTipoCuenta(TipoCuenta tipoCuenta);
    }

    public class RepositorioTiposCuentas : IRepositorioCuentas
    {
        private readonly string connectionString;

        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("TiposCuentas_Insertar", 
                new {usuarioId = tipoCuenta.UsuarioId, nombre = tipoCuenta.Nombre},
                commandType: System.Data.CommandType.StoredProcedure);
            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string Nombre, int UsuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var exists = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM TiposCuentas
                                                                WHERE Nombre = @Nombre
                                                                AND UsuarioId = @UsuarioId;", 
                                                                new {Nombre, UsuarioId});
            return exists == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> GetTiposCuentas(int UsuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden FROM TiposCuentas WHERE UsuarioId = @UsuarioId ORDER BY Orden;", new {UsuarioId});
        }

        public async Task UpdateTipoCuenta(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE TiposCuentas SET Nombre = @Nombre WHERE Id = @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> GetById(int Id, int UsuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"SELECT Nombre, Orden, Id
                                                        FROM TiposCuentas WHERE UsuarioId = @UsuarioId AND Id = @Id;", new {Id, UsuarioId});
        }

        public async Task Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE TiposCuentas WHERE Id = @Id;", new {id});
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados)
        {
             using var connection = new SqlConnection(connectionString);
             await connection.ExecuteAsync(@"UPDATE TiposCuentas SET Orden = @Orden WHERE Id = @Id;", tipoCuentasOrdenados);
        }

    }
}
