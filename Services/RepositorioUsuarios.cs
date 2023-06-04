using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioUsuarios
    {
        Task<int> CrearUsuario(Usuario usuario);
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
    }

    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string connectionString;
        public RepositorioUsuarios(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var connection = new SqlConnection(connectionString);
            var UsuarioId = await connection.QuerySingleAsync<int>("INSERT INTO Usuarios (Email, EmailNormalizado, PasswordHash) VALUES (@Email, @EmailNormalizado, @PasswordHash) SELECT SCOPE_IDENTITY()", usuario);

            await connection.ExecuteAsync("dataPorDefecto", new {UsuarioId}, commandType: System.Data.CommandType.StoredProcedure);

            return UsuarioId;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(connectionString);
            var usuario = await connection.QuerySingleOrDefaultAsync<Usuario>("SELECT * FROM Usuarios WHERE EmailNormalizado = @emailNormalizado;", new {emailNormalizado});
            return usuario;
        }
    }
}