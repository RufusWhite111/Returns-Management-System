using NEAProjectMain.SQLSelectClasses;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace NEAProjectMain
{
    //I have created a controller class so I do not need to pass the connection in to the GUI
    public class Controller : IDisposable
    {
        private SqlConnection connection;
        private string selectedSpreadsheetID;
        private DataTable rejectedData;



        // Constructor to initialise the controller
        public Controller()
        {
            connection = null;
            selectedSpreadsheetID = null;
            rejectedData = null;
        }
        //gets the data from ProcessingAPI and stores the connections and rejectedData. Accepted data not needed as stored in SQL Server
        public void ProcessSelectedDatabase(string spreadsheetID)
        {
            selectedSpreadsheetID = spreadsheetID;

            var (acceptData, rejectData) = ProcessingAPIData.ProcessingAPIDataMain(spreadsheetID);

            rejectedData = rejectData;


            connection = ConnectingToSQL.SQLConnection(spreadsheetID);

            ConnectingToSQL.InsertDataIntoSQL(connection, acceptData);
        }

        //Creates summary of database and is automatically called
        public string GetDatabaseSummary()
        {
            if (connection == null)
            {
                throw new Exception("No database has been selected or processed.");
            }


            try
            {
                var (startDate, endDate) = SQLSelect.RangeOfDates(connection);

                int totalReturns = SQLSelect.GetNumOfReturns(connection);
                var topReturnReasons = SQLSelect.RankReturnReasons(connection);

                //used ToShortDateString as don't need the full information as only Date and hours+minutes of time is needed

                string summary = $"Data Range: {startDate.ToShortDateString()} to {endDate.ToShortDateString()}\n\n" +
                                $"Total Returns: {totalReturns}\n\n" +
                                "Top Return Reasons:\n";

                foreach (var reason in topReturnReasons)
                {
                    // used regex to replace '_' with a space as it looks far better in the returns summary

                    string addingSpaces = System.Text.RegularExpressions.Regex.Replace(reason.ReturnReason, "_", " ");

                    //uses a positive look behind assertion to identify the find character in each word and then adjusts the rest of the characters to lower case
                    string adjustingCaps = Regex.Replace(addingSpaces, @"(?<=\b\w)(\w+)", match => match.Groups[1].Value.ToLower());

                    summary += $"- {adjustingCaps}: {reason.ReturnCount} returns\n";
                }

                return summary;
            }
            catch (Exception e)
            {
                throw new Exception($"Error fetching database summary: {e.Message}");
            }
        }


        public List<MonthlyReturnCount> GetMonthlyReturnCounts()
        {
            if (connection == null)
            {
                throw new Exception("No database has been selected or processed.");

            }

            return SQLSelect.GetMonthlyReturnCounts(connection);
        }
        public int GetTotalReturns()
        {
            if (connection == null)
            {
                throw new Exception("No database has been selected or processed.");
            }

            return SQLSelect.GetNumOfReturns(connection);
        }

        public List<ExchangeVsReturnCount> GetExchangeVsReturnCounts()
        {
            if (connection == null)
            {
                throw new Exception("No database has been selected or processed.");
            }

            return SQLSelect.NumOfExchangeVsReturnCount(connection);
        }

        public int GetOrderCountBetweenDates(DateTime startDate, DateTime endDate)
        {
            if (connection == null)
            {
                throw new Exception("No database has been selected or processed.");
            }

            return SQLSelect.NumOfOrdersBetweenDates(connection, startDate, endDate);
        }
        public List<RankedItem> GetRankedItems(string columnName)
        {
            if (connection == null)
            {
                throw new Exception("No database has been selected or processed.");
            }

            return SQLSelect.RankReturnItems(connection, columnName);
        }


        public List<RankedReturnReason> GetRankedReturnReasons()
        {
            if (connection == null)
            {
                throw new Exception("No database has been selected or processed.");
            }

            return SQLSelect.RankReturnReasons(connection);
        }
        public IndividualCustomerOrderSummary GetCustomerOrders(string customerName)
        {
            if (connection == null)
            {
                throw new Exception("No database has been selected or processed.");
            }

            return SQLSelect.GetAllCustomerOrders(connection, customerName);
        }


        public IndividualOrderDetails GetOrderDetails(int orderID)
        {
            if (connection == null)
            {
                throw new Exception("No database has been selected or processed.");
            }

            return SQLSelect.GetOrderDetails(connection, orderID);

        }
        public DataTable GetRejectedData()
        {
            if (rejectedData == null)
            {
                throw new Exception("No rejected data is available.");

            }

            return rejectedData;
        }


        public void Dispose()
        {
            connection?.Close();
            connection?.Dispose();
        }

    }
}
