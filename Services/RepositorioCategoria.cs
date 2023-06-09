using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioCategoria
    {
        Task CrearCategoria(Categoria categoria);
        Task<IEnumerable<Categoria>> ObtenerCategorias(int usuarioId, PaginacionViewModel paginacion);
        Task<Categoria> ObtenerUnaCategoria(int id, int usuarioId);
        Task EditarCategoria(Categoria categoria);
        Task BorrarCategoria(int id);
        Task<IEnumerable<Categoria>> ObtenerCategoriaOperacion(int usuarioId, TipoOperacion tipoOperacionId);
        Task<int> Contar(int usuarioId);
    }

    public class RepositorioCategoria : IRepositorioCategoria
    {
        private readonly string connectionString;

        public RepositorioCategoria(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConection");
        }

        public async Task CrearCategoria(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Categorias (Nombre, TipoOperacionId, UsuarioId) VALUES (@Nombre, @TipoOperacionId, @UsuarioId);
            SELECT SCOPE_IDENTITY();", categoria);

            categoria.Id = id;
        }

        public async Task<int> Contar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var numeroPaginas = await connection.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM Categorias WHERE UsuarioId = @usuarioId;", new {usuarioId});

            return numeroPaginas;
        }

        public async Task<IEnumerable<Categoria>> ObtenerCategorias(int usuarioId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            var categorias = await connection.QueryAsync<Categoria>(@$"SELECT * FROM Categorias WHERE UsuarioId = @UsuarioId ORDER BY Nombre OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT {paginacion.RecordsPorPagina} ROWS ONLY;", new {usuarioId});

            return categorias;
        }

        public async Task<IEnumerable<Categoria>> ObtenerCategoriaOperacion(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            var categorias = await connection.QueryAsync<Categoria>(@"SELECT * FROM Categorias WHERE UsuarioId = @UsuarioId AND TipoOperacionId = @tipoOperacionId;", new {usuarioId, tipoOperacionId});

            return categorias;
        }

        public async Task<Categoria> ObtenerUnaCategoria(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var categoria = await connection.QueryFirstOrDefaultAsync<Categoria>(@"SELECT * FROM Categorias WHERE UsuarioId = @usuarioId AND Id = @id;", new {id, usuarioId});

            return categoria;
        }

        public async Task EditarCategoria(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Categorias SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId WHERE Id = @Id;", categoria);
        }

        public async Task BorrarCategoria(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Categorias WHERE Id = @Id;", new {id});
        }
    }
}
