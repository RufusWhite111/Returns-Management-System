using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using NEAProjectMain.SQLSelectClasses;
using System.Runtime.CompilerServices;

namespace NEAProjectMain
{
    //: IDisposable
    public class SQLSelect
    {
        //General SQL Requsts

        public static List<RankedItem> RankReturnItems(SqlConnection sqlConnection,string columnName)
        {
            var results = new List<RankedItem>();


            string query = SQLSelectQueries.GetRankReturnItemsQuery(columnName);

            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read() == true)
                    {

                        results.Add(new RankedItem
                        {
                            Item = reader[columnName].ToString(),
                            ReturnCount = Convert.ToInt32(reader["ReturnCount"]),
                            Rank = Convert.ToInt32(reader["Rank"])
                        }
                        );
                    }


                }
            }

            return results;

        }

        public static List<RankedReturnReason> RankReturnReasons(SqlConnection sqlConnection)
        {


            List<RankedReturnReason> results = new();

            string query = SQLSelectQueries.RankReturnReasonsQuery;

            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read() == true)

                    {
                        results.Add(new RankedReturnReason
                        {

                            ReturnReason = reader["ReturnReason"].ToString(),
                            ReturnCount = Convert.ToInt32(reader["ReturnCount"]),
                            Rank = Convert.ToInt32(reader["Rank"])
                        });
                    }
                }
            }

            return results;
        }


        public static List<ExchangeVsReturnCount> NumOfExchangeVsReturnCount(SqlConnection sqlConnection)
        {
            List<ExchangeVsReturnCount> results = new List<ExchangeVsReturnCount>();

            string query = SQLSelectQueries.ExchangeVSReturn;

            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read() ==  true)
                    {
                        results.Add(new ExchangeVsReturnCount
                        {

                            //testing out using ? as if the value is null then it will show an exception, instead it will return a 0

                            NumofExchanges = reader["ExchangeCount"] != DBNull.Value ? Convert.ToInt32(reader["ExchangeCount"]) : 0,
                            NumofReturns = reader["ReturnCount"] != DBNull.Value ? Convert.ToInt32(reader["ReturnCount"]) : 0


                        });
                    }
                }
            }

            return results;
        }

        public static (DateTime startDate, DateTime endDate) RangeOfDates(SqlConnection connection)
        {

            //means that if no data exists, will just return the maximum data and minimum date and so code can continue to run
            DateTime startDate = DateTime.MaxValue;
            DateTime endDate = DateTime.MinValue;

            string query = SQLSelectQueries.RangeOfDatesQuery;


            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read() == true)
                    {
                        if ((reader["StartDate"] == DBNull.Value) || (reader["EndDate"] == DBNull.Value))
                        {
                            throw new Exception("No data exists in the ReturnOrders table.");
                        }

                        startDate = Convert.ToDateTime(reader["StartDate"]);

                        endDate = Convert.ToDateTime(reader["EndDate"]);
                    }
                    else
                    {
                        throw new Exception("No data exists in the ReturnOrders table.");
                    }
                }
            }

            return (startDate, endDate);
        }


        public static List<MonthlyReturnCount> GetMonthlyReturnCounts(SqlConnection connection)
        {
            List<MonthlyReturnCount> results = new List<MonthlyReturnCount>();

            //so I know how many months to include in my barchart and I know the width
            var (startDate, endDate) = RangeOfDates(connection);

            string query = SQLSelectQueries.MonthlyReturnCountQuery;


            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read() == true)
                    {
                        results.Add(new MonthlyReturnCount
                        {
                            Year = Convert.ToInt32(reader["Year"]),
                            Month = Convert.ToInt32(reader["Month"]),
                            ReturnCount = Convert.ToInt32(reader["ReturnCount"])
                        });
                    }
                }
            }

            return results;
        }

        //Specific SQL requests

        public static IndividualOrderDetails GetOrderDetails(SqlConnection connection, int orderId)
        {
            var orderDetails = new IndividualOrderDetails();
            var items = new List<ItemInOrder>(); // Temporary list for items

            using (SqlCommand command = new SqlCommand(SQLSelectQueries.GetOrderDetailsQuery, connection))
            {
                command.Parameters.AddWithValue("@OrderId", orderId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new Exception("No orders match the OrderId entered");
                    }

                    int rowCount = 0;
                    while (reader.Read())
                    {
                        // Store order-level information once
                        if (orderDetails.CustomerName == null)
                        {
                            orderDetails.CustomerName = reader["CustomerName"].ToString();
                            orderDetails.CreatedAt = (DateTime)reader["CreatedAt"];
                            orderDetails.OriginalPaymentMethod = reader["OriginalPaymentMethod"].ToString();
                        }

                        // Collect all items
                        items.Add(new ItemInOrder
                        {
                            ProductName = reader["ProductName"].ToString(),
                            ProductColour = reader["ProductColour"].ToString(),
                            ProductSize = reader["ProductSize"].ToString(),
                            ReturnReason = reader["ReturnReason"].ToString(),
                            IsExchange = (bool)reader["IsExchange"]
                        });

                        rowCount++;
                    }

                    // Only take the first half of the items
                    orderDetails.Items.AddRange(items.Take(rowCount / 2));
                }
            }

            return orderDetails;
        }


        public static IndividualCustomerOrderSummary GetAllCustomerOrders(SqlConnection sqlConnection, string customerName)
        {
            try
            {
                //am using to validate customername format as should ensure that any SQL injection had to be short with a weird structure
                CheckCustomerName(customerName);
            }
            catch (Exception e)
            {

                Console.WriteLine($"Customer name can not be found: {e.Message}");

                return new IndividualCustomerOrderSummary 
                { 
                    TotalOrders = 0
                    
                }; 
            }

            List<IndividualCustomerOrder> orders = new();
            string query = SQLSelectQueries.GetCustomerOrdersQuery;


            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                //should prevent any SQL injection but will need to test further
                command.Parameters.AddWithValue("@CustomerName", customerName);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        orders.Add(new IndividualCustomerOrder
                        {
                            OrderId = Convert.ToInt32(reader["OrderId"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])


                        });
                    }


                }
            }

            return new IndividualCustomerOrderSummary
            {
                TotalOrders = orders.Count,
                Orders = orders
            };

        }
        public static int NumOfOrdersBetweenDates(SqlConnection sqlConnection, DateTime startDate, DateTime endDate)
        {
            //should not need to check structure of start and end date as they have to adhere to the DateTime structure

            if (startDate > endDate)
            {
                throw new Exception("The start date can not be later then the end date");
            }
            int orderCount = 0;

            string query = SQLSelectQueries.NumOfOrdersBetweenDatesQuery;
            try
            {


                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);

                    //only returns one int so I can use ExecuteScalar to speed up operation
                    orderCount = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"There was an issue checking the number of orders between {startDate} and {endDate}: {e.Message}");
            }
            return orderCount;
        }


        public static int GetNumOfReturns(SqlConnection sqlConnection)
        {
            int totalReturns = 0;

            string query = SQLSelectQueries.NumOfReturnsQuery;

            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    totalReturns = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error retrieving total returns: {e.Message}");
            }

            return totalReturns;
        }
        public static void CheckCustomerName(string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName) ==   true)
            {

                throw new ArgumentException("Customer name cannot be empty or just spaces.");
            }

            string[] nameParts = customerName.Trim().Split(' ');
            // the structure has to be FirstName Surname
            if  (nameParts.Length != 2)
            {
                throw new ArgumentException("Customer name must contain a first name and a surname separated by a space.");
            }
        }

        
    }




}






