
using System.Data.SqlClient;
using Zad6.Exceptions;

namespace Zad6.Repositories;

public interface IWarehouseRepository
{
    public Task<int?> RegisterProductInWarehouseAsync(int idWarehouse, int idProduct, int idOrder,int amout, DateTime createdAt);
    public Task RegisterProductInWarehouseByProcedureAsync(int idWarehouse, int idProduct, int Amount);
}

public class WarehouseRepository : IWarehouseRepository
{
    private readonly IConfiguration _configuration;

    public WarehouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task<int?> RegisterProductInWarehouseAsync(int idWarehouse, int idProduct,int idOrder, int amout, DateTime createdAt)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var query = "SELECT * from \"Product\" where IdProduct = @IdProduct";
            await using var command = new SqlCommand(query, connection);
            command.Transaction = (SqlTransaction)transaction;
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            using var reader = await command.ExecuteReaderAsync();
            bool product = false;

            if (reader.HasRows)
            {
                product = true;
            }
            reader.Close();

            command.CommandText = "SELECT * from Warehouse where IdWarehouse = @IdWarehouse";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            using var reader2 = await command.ExecuteReaderAsync();
            bool warehouse = false;

            if (reader2.HasRows)
            {
                warehouse = true;
            }
            reader2.Close();

            if (product == false || warehouse == false)
            {
                throw new NotFoundException("");
            }

            command.CommandText = "SELECT * from Order where IdOrder = @IdOrder and Amount = @Amount";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Amount", amout);
            using var reader3 = await command.ExecuteReaderAsync();
            if (!reader3.HasRows)
            {
                throw new NotFoundException("");
            }
            reader3.Close();

            command.CommandText = "SELECT * from Product_Warehouse where IdOrder = @IdOrder";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            using var reader4 = await command.ExecuteReaderAsync();
            if (reader4.HasRows)
            {
                throw new ConflictException("");
            }
            reader4.Close();

            command.CommandText = "UPDATE Order SET FulfilledAt = GETDATE() where IdOrder = @IdOrder";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            await command.ExecuteNonQueryAsync();

            command.CommandText =
                "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, CreatedAt, Amount, Price, CreatedAt) " +
                "OUTPUT Inserted.IdProductWarehouse VALUES (@IdWarehouse, @IdProduct, @IdOrder, @CreatedAt, @Amount, 0,@CreateAt)";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            command.Parameters.AddWithValue("@Amount", amout);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            var idProductWarehouse = (int)await command.ExecuteScalarAsync();

            await transaction.CommitAsync();
            return idProductWarehouse;
        }
        catch (NotFoundException e)
        {
            await transaction.RollbackAsync();
            return null;
        }

    }

    public Task RegisterProductInWarehouseByProcedureAsync(int idWarehouse, int idProduct, int Amount)
    {
        throw new NotImplementedException();
    }
}