using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAProjectMain
{
    public class SQLInsert
    {

        public static DataTable InsertProcessedData(SqlConnection sqlConnection, DataTable dataToInsert, DataTable rejectData)
        {
            //used SQLTransaction with commit and roleback to ensure the data is fully correct before inserting as I found that otherwise I end up with a half filled database

            if (dataToInsert == null || dataToInsert.Rows.Count == 0)
            {
                throw new Exception("There is no valid data to insert");
            }

            using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
            {
                try
                {
                    foreach (DataRow dataRow in dataToInsert.Rows)
                    {
                        try
                        {
                            int customerId = InsertCustomer(sqlConnection, sqlTransaction, dataRow["customer_name"].ToString());
                            int orderId = InsertOrder(sqlConnection, sqlTransaction, dataRow, customerId);
                            string productId = InsertProduct(sqlConnection, sqlTransaction, dataRow);
                            int returnOrderId = InsertReturnOrder(sqlConnection, sqlTransaction, dataRow, orderId);

                            InsertReturnItem(sqlConnection, sqlTransaction, dataRow, returnOrderId, productId);

                        }
                        catch (Exception e)
                        {

                            Console.WriteLine($"Error inserting row: {e.Message}");


                            DataRow rejectRow = rejectData.NewRow();
                            rejectRow["RejectedData"] = string.Join(", ", dataRow.ItemArray);
                            rejectRow["RejectionReason"] = $"Failed to insert due to: {e.Message}";
                            rejectData.Rows.Add(rejectRow);

                            Console.WriteLine("Added row to rejectRow");

                            //can ignore this row because while the relational Database is not completely correct, one row in the grand scheme is not make or break
                            continue;

                        }
                       
                    }
                    sqlTransaction.Commit();
                    Console.WriteLine("All the data inserted successfully, hooray");
                }
                catch (Exception e)
                {
                    sqlTransaction.Rollback();
                    Console.WriteLine($"Transaction rolled back: {e.Message}");
                }

            }
            return rejectData;
        }
        
        private static object RetrieveSQLData(SqlConnection sqlConnection, SqlTransaction sqlTransaction, string query, List<SqlParameter> parameters)
        {
            using SqlCommand commandSQL = new SqlCommand(query, sqlConnection, sqlTransaction);
            commandSQL.Parameters.AddRange(parameters.ToArray());

            object result = commandSQL.ExecuteScalar();           
            if (result == DBNull.Value)
            {
                return null;
            }
            else
            {
                return result;
            }
        }

        private static void RunNonQuery(SqlConnection sqlConnection, SqlTransaction sqlTransaction, string query, List<SqlParameter> parameters)
        {
            using SqlCommand commandSQL = new SqlCommand(query, sqlConnection, sqlTransaction);

            commandSQL.Parameters.AddRange(parameters.ToArray());
            commandSQL.ExecuteNonQuery();
        }

        private static int InsertCustomer(SqlConnection sqlConnection, SqlTransaction sqlTransaction, string customerName)
        {
            List<SqlParameter> selectParameters = new List<SqlParameter>
            {
                new SqlParameter("@CustomerName", customerName)
            };


            object existingId = RetrieveSQLData(sqlConnection, sqlTransaction, SQLInsertQueries.SelectCustomerQuery, selectParameters);


            if (existingId != null)
            {
                return Convert.ToInt32(existingId);
            }

            List<SqlParameter> insertParamaters = new List<SqlParameter>
            {
            new SqlParameter("@CustomerName", customerName)
            }; 


            return Convert.ToInt32(RetrieveSQLData(sqlConnection, sqlTransaction, SQLInsertQueries.InsertCustomerQuery, insertParamaters));
        }

        private static int InsertOrder(SqlConnection sqlConnection, SqlTransaction sqlTransaction, DataRow dataRow, int customerId)
        {
            int orderId = Convert.ToInt32(dataRow["order_name"]);

            List<SqlParameter> selectParams = new List<SqlParameter> 
            { 
                new SqlParameter("@OrderId", orderId) 
            };

            

                object existingId = RetrieveSQLData(sqlConnection, sqlTransaction, SQLInsertQueries.SelectOrderQuery, selectParams);

            if (existingId != null)
            {
                return orderId;
            }

            List<SqlParameter> insertParams = new List<SqlParameter>
            {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@OriginalPaymentMethod", dataRow["credit_method"].ToString()),
                new SqlParameter("@CreatedAt", Convert.ToDateTime(dataRow["created_at"]))
            };

            RunNonQuery(sqlConnection, sqlTransaction, SQLInsertQueries.InsertOrderQuery, insertParams);
           
            
            return orderId;
        }

        private static string InsertProduct(SqlConnection sqlConnection, SqlTransaction sqlTransaction, DataRow dataRow)
        {
            string productId = dataRow["sku"].ToString();

            List<SqlParameter> selectParamater = new List<SqlParameter> 
            { 
                new SqlParameter("@ProductId", productId) 
            };


            object existingId = RetrieveSQLData(sqlConnection, sqlTransaction, SQLInsertQueries.SelectProductQuery, selectParamater);

            if (existingId != null)
            {
                return productId;
            }

            List<SqlParameter> insertParamaters = new List<SqlParameter>
            {
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@ProductName", dataRow["product_name"].ToString()),
                new SqlParameter("@ProductColour", dataRow["product_colour"].ToString()),
                new SqlParameter("@ProductSize", dataRow["product_size"].ToString())
            };


            object result = RetrieveSQLData(sqlConnection, sqlTransaction, SQLInsertQueries.InsertProductQuery, insertParamaters);

            if (result == null)
            {
                return productId;
            }
            else
            {
                return result.ToString();
            }
        }



        private static int InsertReturnOrder(SqlConnection sqlConnection, SqlTransaction sqlTransaction, DataRow dataRow, int orderId)
        {
            List<SqlParameter> insertParamaters = new List<SqlParameter>
            {
                new SqlParameter("@OrderId", orderId),

                new SqlParameter("@CreditMethod", dataRow["credit_method"].ToString()),
                new SqlParameter("@CreatedAt", Convert.ToDateTime(dataRow["created_at"]))
            };
           


            return Convert.ToInt32(RetrieveSQLData(sqlConnection, sqlTransaction, SQLInsertQueries.InsertReturnOrderQuery, insertParamaters));
        }

        private static void InsertReturnItem(SqlConnection sqlConnection, SqlTransaction sqlTransaction, DataRow dataRow, int returnOrderId, string productId)
        {
            List<SqlParameter> insertParameters = new List<SqlParameter>()
            {
                new SqlParameter("@ReturnOrderId", returnOrderId),
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@ReturnReason", dataRow["return_reason"].ToString()),
                new SqlParameter("@ReturnNote", dataRow["return_note"].ToString()),
                new SqlParameter("@IsExchange", Convert.ToBoolean(dataRow["is_exchange"]))
            };

            RunNonQuery(sqlConnection, sqlTransaction, SQLInsertQueries.InsertReturnItemQuery, insertParameters);

        }
    }
}
    


