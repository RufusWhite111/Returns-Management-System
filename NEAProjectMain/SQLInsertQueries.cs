using System;

public static class SQLInsertQueries
{
    //apparently meant to use PascalCasing as the naming convention for constants according to stack overflow
    //https://stackoverflow.com/questions/242534/c-sharp-naming-convention-for-constants

    public const string SelectCustomerQuery = "SELECT CustomerId FROM Customers WHERE CustomerName = @CustomerName";
    public const string InsertCustomerQuery = "INSERT INTO Customers (CustomerName) OUTPUT INSERTED.CustomerId VALUES (@CustomerName)";

    public const string SelectOrderQuery = "SELECT OrderId FROM Orders WHERE OrderId = @OrderId";
    public const string InsertOrderQuery = @"INSERT INTO Orders (OrderId, CustomerId, OriginalPaymentMethod, CreatedAt) 
                                             VALUES (@OrderId, @CustomerId, @OriginalPaymentMethod, @CreatedAt)";

    public const string SelectProductQuery = "SELECT ProductId FROM Products WHERE ProductId = @ProductId";

    public const string InsertProductQuery = @"INSERT INTO Products (ProductId, ProductName, ProductColour, ProductSize) 
                                              OUTPUT INSERTED.ProductId VALUES (@ProductId, @ProductName, @ProductColour, @ProductSize)";

    public const string InsertReturnOrderQuery = @"INSERT INTO ReturnOrders (OrderId, CreditMethod, CreatedAt) 
                                                 OUTPUT INSERTED.ReturnOrderId VALUES (@OrderId, @CreditMethod, @CreatedAt)";

    public const string InsertReturnItemQuery = @"INSERT INTO ReturnItems (ReturnOrderId, ProductId, ReturnReason, ReturnNote, IsExchange) 
                                                 VALUES (@ReturnOrderId, @ProductId, @ReturnReason, @ReturnNote, @IsExchange)";
}
