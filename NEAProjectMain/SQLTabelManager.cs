using System;
using System.Data.SqlClient;

public class SQLTableManager
    //used to initialise all the tables
{

    public static void DropTables(SqlConnection connection)
    {

        CheckConnectionIsOpen(connection);
        

        //hard coded so not vunerable to SQL injections, better to be safe than sorry

        string[] tables = { "ReturnItems", "ReturnOrders", "Products", "Orders", "Customers"};


        foreach (var table in tables)
        {
            string sql = $"DROP TABLE IF EXISTS {table};";
            ExecuteDropTableFunctions(sql, connection);
        }

        Console.WriteLine("Successfully dropped all tables");
    }

    public static void CreateTables(SqlConnection connection)
    {
        CheckConnectionIsOpen(connection);

        ExecuteCreateTableFunctions(CreateCustomersTable(), connection);
        ExecuteCreateTableFunctions(CreateOrdersTable(), connection);
        ExecuteCreateTableFunctions(CreateProductsTable(), connection);
        ExecuteCreateTableFunctions(CreateReturnOrdersTable(), connection);
        ExecuteCreateTableFunctions(CreateReturnItemsTable(), connection);
    }


    //found it was quicker and neater to have one main function to Insert the text instead of a repeating SQL calls
    //also tried ExecuteReader but found ExecuteNonQuery quicker because it does not expect any results

    private static void ExecuteDropTableFunctions(string sqlCommand, SqlConnection connection)
    {
        using (SqlCommand command = new SqlCommand(sqlCommand, connection))
        {
            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine("Table dropped without errors");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when dropping table: {e.Message}");
            }
        }
    }
    private static void ExecuteCreateTableFunctions(string sqlCommand, SqlConnection connection)
    {
        using (SqlCommand command = new SqlCommand(sqlCommand, connection))
        {
            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine("Table created without errors");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when creating table: {e.Message}");
            }
        }
    }

    private static void CheckConnectionIsOpen(SqlConnection connection)
    {
        try
        {


            if (connection.State != System.Data.ConnectionState.Open)
            {

                connection.Open();
            }
        }
        catch (SqlException e)
        {
            Console.WriteLine($"Issues connecting with SQL:{e.Message}");

        }
        catch (Exception e)
        {
            Console.WriteLine($"Error when opening connection: {e.Message}");
        }
    }
    private static string CreateCustomersTable()
    {
        string createCustomersTable = @" CREATE TABLE Customers (CustomerId INT PRIMARY KEY IDENTITY(1,1),CustomerName VARCHAR(100) NOT NULL )";

        return createCustomersTable;
    }

    private static string CreateOrdersTable()
    {
        string createOrdersTable = @"CREATE TABLE Orders (OrderId INT PRIMARY KEY, CustomerId INT NOT NULL, 
                                    OriginalPaymentMethod VARCHAR(50) NOT NULL, CreatedAt DATETIME NOT NULL, 
                                    FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId))";
        return createOrdersTable;

       
    }

    private static string CreateProductsTable()
    {
        string createProductsTable = @"CREATE TABLE Products (ProductId VARCHAR(11) PRIMARY KEY, ProductName VARCHAR(100) NOT NULL, 
                                       ProductColour VARCHAR(10) NOT NULL,ProductSize VARCHAR(10) NOT NULL)";
        return createProductsTable;
    }




        private static string CreateReturnOrdersTable()
    {
        string createReturnOrderTable = @"CREATE TABLE ReturnOrders (ReturnOrderId INT PRIMARY KEY IDENTITY(1,1), OrderId INT NOT NULL, 
                                          CreditMethod VARCHAR(50), CreatedAt DATETIME NOT NULL, 
                                         FOREIGN KEY (OrderId) REFERENCES Orders(OrderId))";
        return createReturnOrderTable;
    }

    private static string CreateReturnItemsTable()
    {
        string createReturnItemsTable = @"CREATE TABLE ReturnItems (ReturnItemId INT PRIMARY KEY IDENTITY(1,1),ReturnOrderId INT NOT NULL, 
                                        ProductId VARCHAR(11) NOT NULL,ReturnReason VARCHAR(50) NOT NULL, ReturnNote VARCHAR(500), 
                                        IsExchange BIT NOT NULL, FOREIGN KEY (ReturnOrderId) REFERENCES ReturnOrders(ReturnOrderId), 
                                        FOREIGN KEY (ProductId) REFERENCES Products(ProductId))";
        return createReturnItemsTable;
    }




}