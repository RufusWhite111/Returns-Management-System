using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using NEAProjectMain.SQLSelectClasses;



namespace NEAProjectMain
{
    public static class ConnectingToSQL
    {
        //has to return SQLConnection so other Queries can be called from the GUI and rejectData is needed to be returned if the user wants it
        public static SqlConnection  SQLConnection(string sheetID)
        {
            DataTable acceptData = null;
            DataTable rejectData = null;

            try
            {
                //gets the data from the PI
                (acceptData, rejectData) = ProcessingAPIData.ProcessingAPIDataMain(sheetID);

                //loads the connection string from the JSON configuration file
                var connectionStringJSON = new ConfigurationBuilder().AddJsonFile("SettingsProgram.json", optional: false).Build();

                string connectionString = connectionStringJSON["DataBaseConfig:ConnectionString"] ?? string.Empty;

                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("The SQL filePath can not be found");
                    return null;
                }

                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                Console.WriteLine("Connection open");

                // Drops and creates tables in the database
                SQLTableManager.DropTables(connection);
                SQLTableManager.CreateTables(connection);

                // Insert processed data into SQL
                rejectData = SQLInsert.InsertProcessedData(connection, acceptData, rejectData);

                // Fetch ranked data for testing
                List<RankedItem> productSizesRanked = SQLSelect.RankReturnItems(connection, "ProductSize");
                List<RankedItem> productRanked = SQLSelect.RankReturnItems(connection, "ProductName");
                List<RankedItem> productColourRanked = SQLSelect.RankReturnItems(connection, "ProductColour");
               
                
                // Dispose of the acceptData table to free up memory
                acceptData.Dispose();
                
                //returns the connection so the user can make
                return connection;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue receiving the processed data: {e.Message}");
                return null;
            }
            finally
            {
                // Dispose of the DataTables to free up memory
                if (acceptData != null)
                {
                    acceptData.Dispose();
                }  
            }
        }

        
        public static void InsertDataIntoSQL(SqlConnection connection, DataTable acceptData)
        {
            try
            {
                SQLInsert.InsertProcessedData(connection, acceptData, null);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error inserting data into SQL: {e.Message}");
            }
        }

        //functions below are used for testing the output of SQLSelect Queries

        
        public static void DisplayOrderDetails(IndividualOrderDetails orderDetails, int orderId)
        {
            Console.WriteLine($"Order Details for Order ID: {orderId}");

            if (orderDetails == null || orderDetails.TotalItemsInOrder == 0)
            {
                Console.WriteLine("No order found with this ID. Please check the ID and try again.");
                return;
            }

            Console.WriteLine($"Customer: {orderDetails.CustomerName}");
            Console.WriteLine($"Created At: {orderDetails.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Payment Method: {orderDetails.OriginalPaymentMethod}");

            foreach (var item in orderDetails.Items)
            {
                Console.WriteLine($"Product: {item.ProductName}, Colour: {item.ProductColour}, Size: {item.ProductSize}");
                Console.WriteLine($"Return Reason: {item.ReturnReason}, Exchange: {(item.IsExchange ? "Yes" : "No")}");
            }

            Console.WriteLine($"Total Items Returned: {orderDetails.TotalItemsInOrder}");
        }

        
        public static void DisplayRankedItems(IEnumerable<RankedItem> rankedItems, string itemType)
        {
            foreach (var item in rankedItems)
            {
                Console.WriteLine($"{itemType}: {item.Item}, Return Count: {item.ReturnCount}, Rank: {item.Rank}");
            }
        }


        
        public static void DisplayCustomerOrders(IndividualCustomerOrderSummary customerOrders, string customerName)
        {
            Console.WriteLine($"Customer: {customerName}");
            Console.WriteLine($"Total Orders: {customerOrders.TotalOrders}");

            if (customerOrders.TotalOrders > 0)
            {
                Console.WriteLine("Order Details:");
                foreach (var order in customerOrders.Orders)
                {
                    Console.WriteLine($"Order ID: {order.OrderId}, Created At: {order.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                }
            }
            else
            {
                Console.WriteLine("Unfortunately, no orders found for this customer. Please check the name and try again.");
            }
        }
    }
}