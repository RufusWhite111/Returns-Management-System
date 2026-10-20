using System;

public static class SQLSelectQueries
{
    public static string GetRankReturnItemsQuery(string columnName)
    {
        return $@" SELECT p.{columnName}, COUNT(ri.ReturnItemId) AS ReturnCount, RANK() OVER 
                  (ORDER BY COUNT(ri.ReturnItemId) DESC) AS Rank FROM ReturnItems ri 
                  JOIN Products p ON ri.ProductId = p.ProductId GROUP BY p.{columnName} ORDER BY ReturnCount DESC";
    }
    //was able to remove "Order BY RETURNCOUNT DESC" as RANK() already orders by COUNT(ReturnItemId)
    public const string RankReturnReasonsQuery = @"SELECT ReturnReason, COUNT(ReturnItemId) AS ReturnCount, RANK() OVER 
                                                 (ORDER BY COUNT(ReturnItemId) DESC) AS Rank FROM ReturnItems 
                                                  GROUP BY ReturnReason";



    public const string NumOfReturnsQuery = @"SELECT COUNT(*) FROM ReturnItems";

    public const string RangeOfDatesQuery = @"SELECT MIN(CreatedAt) AS StartDate, MAX(CreatedAt) AS EndDate FROM ReturnOrders";

    public const string MonthlyReturnCountQuery = @"SELECT YEAR(CreatedAt) AS Year, MONTH(CreatedAt) AS Month, COUNT(ReturnOrderId) AS ReturnCount 
                                                   FROM ReturnOrders WHERE CreatedAt >= @StartDate AND CreatedAt <= @EndDate GROUP BY YEAR(CreatedAt), 
                                                   MONTH(CreatedAt) ORDER BY Year, Month;";

    public const string GetCustomerOrdersQuery = @"SELECT o.OrderId, o.CreatedAt FROM Orders o JOIN Customers c ON o.CustomerId = c.CustomerId 
                                                   WHERE c.CustomerName = @CustomerName ORDER BY o.CreatedAt DESC";

    public const string GetOrderDetailsQuery = @"SELECT o.OrderId, c.CustomerName, o.CreatedAt, o.OriginalPaymentMethod, p.ProductName, p.ProductColour, 
                                                 p.ProductSize, ri.ReturnReason, ri.IsExchange,(SELECT COUNT(*) FROM ReturnItems 
                                                WHERE ReturnOrderId = ro.ReturnOrderId) AS TotalItems FROM Orders o JOIN Customers c 
                                                ON o.CustomerId = c.CustomerId JOIN ReturnOrders ro ON o.OrderId = ro.OrderId 
                                                JOIN ReturnItems ri ON ro.ReturnOrderId = ri.ReturnOrderId JOIN Products p ON ri.ProductId = p.ProductId 
                                                WHERE o.OrderId = @OrderId 
                                                ORDER BY o.CreatedAt DESC";

    public const string NumOfOrdersBetweenDatesQuery = @"SELECT COUNT(*) FROM Orders WHERE CreatedAt BETWEEN @StartDate AND @EndDate";

    //am using CASE since it means I only need to use one query instead of
    //SUM of IsExchange = 1 and a SUM of IsExchange = 0,
    //discovered about it from https://stackoverflow.com/questions/4622/sql-case-expression-syntax
    public const string ExchangeVSReturn = @"SELECT SUM(CASE WHEN IsExchange = 1 THEN 1 ELSE 0 END) AS ExchangeCount, 
                                            SUM(CASE WHEN IsExchange = 0 THEN 1 ELSE 0 END) AS ReturnCount FROM ReturnItems";


}
