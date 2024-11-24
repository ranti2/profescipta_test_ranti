using Microsoft.Data.SqlClient;
using profescipta_test.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;

namespace profescipta_test.Repository
{
    public class SalesOrderRepo
    {
        private string _connectionString;

        public SalesOrderRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<SalesOrderModel>> GetPagedSalesOrdersAsync(int page, int pageSize, string keyword, string orderDateFilter)
        {
            DateTime? orderDateStart = null;
            DateTime? orderDateEnd = null;

            if (DateTime.TryParse(orderDateFilter, out var parsedDate))
            {

                orderDateStart = parsedDate.Date;
                orderDateEnd = parsedDate.Date.AddDays(1).AddTicks(-1);
            }
            var query = @"
                SELECT id, sales_order, order_date, customer
                FROM [order]
                WHERE(@Keyword = '' OR sales_order LIKE @Keyword OR customer LIKE @Keyword)
                AND (@OrderDateStart IS NULL OR order_date >= @OrderDateStart)
                AND (@OrderDateEnd IS NULL OR order_date <= @OrderDateEnd)
                ORDER BY id DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var salesOrders = new List<SalesOrderModel>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            command.Parameters.AddWithValue("@PageSize", pageSize);
            command.Parameters.AddWithValue("@Keyword", string.IsNullOrEmpty(keyword) ? "" : "%" + keyword + "%");
            command.Parameters.AddWithValue("@OrderDateStart", (object)orderDateStart ?? DBNull.Value);
            command.Parameters.AddWithValue("@OrderDateEnd", (object)orderDateEnd ?? DBNull.Value);
            connection.Open();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                salesOrders.Add(new SalesOrderModel
                {
                    Id = reader.GetInt32(0),
                    SalesOrder = reader.GetString(1),
                    OrderDate = reader.GetDateTime(2),
                    Customer = reader.GetString(3)
                });
            }

            return salesOrders;
        }

        public async Task<int> GetTotalSalesOrderAsync(string keyword, string orderDateFilter)
        {
            int totalRecords = 0;
            DateTime? orderDateStart = null;
            DateTime? orderDateEnd = null;

            if (DateTime.TryParse(orderDateFilter, out var parsedDate))
            {
                // Set the start of the day (00:00) and the end of the day (23:59)
                orderDateStart = parsedDate.Date; // 00:00
                orderDateEnd = parsedDate.Date.AddDays(1).AddTicks(-1); // 23:59:59.999
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var countQuery = @"
                    SELECT COUNT(*)
                    FROM [order]
                    WHERE(@Keyword = '' OR sales_order LIKE @Keyword OR customer LIKE @Keyword)
                    AND (@OrderDateStart IS NULL OR order_date >= @OrderDateStart)
                    AND (@OrderDateEnd IS NULL OR order_date <= @OrderDateEnd)";
                using (var command = new SqlCommand(countQuery, connection))
                {
                    command.Parameters.AddWithValue("@Keyword", string.IsNullOrEmpty(keyword) ? "" : "%" + keyword + "%");
                    command.Parameters.AddWithValue("@OrderDateStart", (object)orderDateStart ?? DBNull.Value);
                    command.Parameters.AddWithValue("@OrderDateEnd", (object)orderDateEnd ?? DBNull.Value);

                    totalRecords = (int)await command.ExecuteScalarAsync();
                }
            }

            return totalRecords;
        }

        // Delete Operation
        public async Task<int> DeleteSalesOrderAsync(int id)
        {
            var query = "DELETE FROM [order] WHERE id = @Id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            return await command.ExecuteNonQueryAsync();
        }

        // Read Operation - Get sales order by id
        public async Task<SalesOrderModel> GetSalesOrderByIdsAsync(int id)
        {
            SalesOrderModel salesOrder = new SalesOrderModel();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT id, sales_order, order_date, customer,address FROM [order] WHERE id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {

                            salesOrder = new SalesOrderModel
                            {
                                Id = !reader.IsDBNull(0) ? reader.GetInt32(0) : 0,
                                SalesOrder = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty,
                                OrderDate = !reader.IsDBNull(2) ? reader.GetDateTime(2) : DateTime.Now,
                                Customer = !reader.IsDBNull(3) ? reader.GetString(3):string.Empty,
                                Address = !reader.IsDBNull(4) ? reader.GetString(4):string.Empty,
                            };
                        }
                    }
                }
            }

            return salesOrder;
        }

        // Read Operation - Get sales order by sales_order
        public async Task<SalesOrderModel> GetSalesOrderByOrdersAsync(string sales_order)
        {
            SalesOrderModel salesOrder = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT id, sales_order, order_date, customer FROM [order] WHERE sales_order = @sales_order";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@sales_order", sales_order);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            salesOrder = new SalesOrderModel
                            {
                                Id = reader.GetInt32(0),
                                SalesOrder = reader.GetString(1),
                                OrderDate = reader.GetDateTime(2),
                                Customer = reader.GetString(3)
                            };
                        }
                    }
                }
            }

            return salesOrder;
        }

        public async Task<List<SalesOrderModel>> GetSalesOrdersAsync(string keyword, string orderDateFilter)
        {
            DateTime? orderDateStart = null;
            DateTime? orderDateEnd = null;

            if (DateTime.TryParse(orderDateFilter, out var parsedDate))
            {
                // Set the start of the day (00:00) and the end of the day (23:59)
                orderDateStart = parsedDate.Date; // 00:00
                orderDateEnd = parsedDate.Date.AddDays(1).AddTicks(-1); // 23:59:59.999
            }
            var query = @"
                SELECT id, sales_order, order_date, customer
                FROM [order]
                WHERE(@Keyword = '' OR sales_order LIKE @Keyword OR customer LIKE @Keyword)
                AND (@OrderDateStart IS NULL OR order_date >= @OrderDateStart)
                AND (@OrderDateEnd IS NULL OR order_date <= @OrderDateEnd )";

            var salesOrders = new List<SalesOrderModel>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Keyword", string.IsNullOrEmpty(keyword) ? "" : "%" + keyword + "%");
            command.Parameters.AddWithValue("@OrderDateStart", (object)orderDateStart ?? DBNull.Value);
            command.Parameters.AddWithValue("@OrderDateEnd", (object)orderDateEnd ?? DBNull.Value);
            connection.Open();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                salesOrders.Add(new SalesOrderModel
                {
                    Id = reader.GetInt32(0),
                    SalesOrder = reader.GetString(1),
                    OrderDate = reader.GetDateTime(2),
                    Customer = reader.GetString(3)
                });
            }

            return salesOrders;
        }

        // Read Operation - Get all orders
        public async Task<List<OrderItemModel>> GetOrderItemAsync(int salesId)
        {

            var query = @"
                SELECT id, sales_order_id,name, qty, price,total
                FROM [order_item]
                WHERE sales_order_id = @SalesId
                ORDER BY id";

            var orderItems = new List<OrderItemModel>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SalesId", salesId);
            connection.Open();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orderItems.Add(new OrderItemModel
                {
                    Id = reader.GetInt32(0),
                    SalesOrderId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Qty = reader.GetInt32(3),
                    Price = reader.GetInt64(4),
                    Total = reader.GetInt64(5)
                });
            }

            return orderItems;
        }

        public async void AddSalesOrderDetail(SalesOrderDetail salesOrderDetail)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int id = await AddSalesOrderAsync(connection, transaction, salesOrderDetail.SalesOrder);
                        BulkInsertOrderItem(connection, transaction, id, salesOrderDetail.Order);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                 }
            } 
         }

        public async void UpdateSalesOrderDetail(SalesOrderDetail salesOrderDetail)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await UpdateSalesOrderAsync(connection, transaction, salesOrderDetail.SalesOrder);
                        BulkDeleteOrderItem(connection, transaction, salesOrderDetail.SalesOrder.Id);
                        BulkInsertOrderItem(connection, transaction, salesOrderDetail.SalesOrder.Id, salesOrderDetail.Order);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
        }

        public async Task<int> AddSalesOrderAsync(SqlConnection connection, SqlTransaction transaction,SalesOrderModel salesOrder)
        {
            var query = "INSERT INTO [order] (sales_order, order_date, customer,address) VALUES (@SalesOrder, @OrderDate, @Customer,@Address); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {

                command.Parameters.AddWithValue("@SalesOrder", salesOrder.SalesOrder);
                command.Parameters.AddWithValue("@OrderDate", salesOrder.OrderDate);
                command.Parameters.AddWithValue("@Customer", salesOrder.Customer);
                command.Parameters.AddWithValue("@Address", salesOrder.Address);
                var id = command.ExecuteScalar();
                Console.WriteLine(id);
                return (int)id;
            }

        }

        public void BulkInsertOrderItem(SqlConnection connection, SqlTransaction transaction,int salesId,List<OrderItemModel> data)
        {

                var sqlBuilder = new System.Text.StringBuilder();
                sqlBuilder.Append("INSERT INTO [order_item] (sales_order_id, name, qty,price,total) VALUES ");

                List<OrderItemModel> filterData = data.Where(o => o.IsTemp == true).ToList();
                int i = 0;
                foreach(OrderItemModel item in filterData)
                {
                    if (item.IsTemp == true)
                    {
                        sqlBuilder.Append($"('{salesId}', '{item.Name}', '{item.Qty}','{item.Price}','{item.Total}')");
                        if (i < filterData.Count - 1)
                        {
                            sqlBuilder.Append(",");
                        }
                        i++;
                    }
                }

                // Execute the command
                using (var command = new SqlCommand(sqlBuilder.ToString(), connection,transaction))
                {
                    command.ExecuteNonQuery();
                }

        }

        public void BulkDeleteOrderItem(SqlConnection connection, SqlTransaction transaction, int salesId)
        {
            string query = "DELETE FROM [order_item] WHERE sales_order_id = @Id";
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", salesId);

                command.ExecuteNonQuery();
            }
        }

        public async Task<int> UpdateSalesOrderAsync(SqlConnection connection, SqlTransaction transaction,SalesOrderModel salesOrder)
        {
            var query = "UPDATE [order] SET sales_order = @SalesOrder, order_date = @OrderDate, customer = @Customer , address = @Address WHERE id = @Id";
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {

                command.Parameters.AddWithValue("@Id", salesOrder.Id);
                command.Parameters.AddWithValue("@SalesOrder", salesOrder.SalesOrder);
                command.Parameters.AddWithValue("@OrderDate", salesOrder.OrderDate);
                command.Parameters.AddWithValue("@Customer", salesOrder.Customer);
                command.Parameters.AddWithValue("@Address", salesOrder.Address != null ? salesOrder.Address : "''");


                return await command.ExecuteNonQueryAsync();
            }
        }
    }
}

