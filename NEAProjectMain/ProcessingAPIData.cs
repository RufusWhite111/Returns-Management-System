using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NEAProjectMain
{
    public class ProcessingAPIData
    {

        //Learnt about DataTables from https://stackoverflow.com/questions/1613817/is-excessive-datatable-usage-bad

        public static (DataTable acceptData,DataTable rejectData) ProcessingAPIDataMain(string sheetID)
        {
            
            //adjust to the number of Columns in the table
            int numOfDataColumns = 9;
            ValueRange result = ReadingAPI.APICall(sheetID);


            //creates DataTables which will store the Reject and Accept Data 
            DataTable rejectData = CreateRejectTable();
            
            DataTable acceptData = CreateAcceptTable(numOfDataColumns);


            try
            {
                if ((result == null) || (result.Values == null))
                {
                    Console.WriteLine("No data returned from API call");
                    
                    //returns empty DataTables so will need to check if they are empty when called
                    return (acceptData, rejectData);
                }
                else
                {

                      //checks to see if there are any missing text
                    foreach (var row in result.Values)
                    {
                        try
                        {


                            if (row.Count == numOfDataColumns)
                            {

                                // add to data structure which gets inserted into sql server
                                InsertIntoDataTable(acceptData, rejectData, row);

                            }
                            else if (row.Count < numOfDataColumns)
                            {
                                //adds to data structure which returns any invalid data
                                HandleOverloadedRow(rejectData, row, $"Less than {numOfDataColumns} columns");
                            }
                            else
                            {
                                //records values to RejectData and stores any extra in an overload table
                                HandleOverloadedRow(rejectData, row, $"More than {numOfDataColumns} columns");

                            }
                        }
                        catch
                        {


                            try
                            {

                                HandleOverloadedRow(rejectData, row, "Error occured when adding data to acceptTable");
                            }

                            catch (Exception e)
                            {
                                 Console.WriteLine($"Issue procesing row {string.Join(",", row)}. {e.Message}");

                            }
                        }

                    }
                    try
                    {
                        SKUProcessing(acceptData, rejectData);
                        DuplicateChecker(acceptData, rejectData);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Issue when processing either SKU or duplicates {e.Message}");
                    }


                    DataChecker(acceptData, rejectData);
                    
                }   
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured during DataHandling{e.Message}");

            }
            return (acceptData, rejectData);

        }

        //checks for duplicates in the acceptData and removes them
        public static void DuplicateChecker(DataTable acceptData, DataTable rejectData)
        {
            
            Dictionary<string, DataRow> duplicateDictionary = new Dictionary<string, DataRow>();
            List<(DataRow row, string reason)> duplicateRows = new();

            //validates the inputs to the function
            if ((acceptData == null)||(rejectData == null))
            {
                Console.WriteLine("Either acceptData or rejectData is missing");
                return;
            }
            if (acceptData.Columns.Contains("order_name") == false)
            {
                Console.WriteLine("Missing order_name");
                return;
                
            }

            string[] columnsForKey = ["created_at", "order_name", "product_line", "product_colour", "product_size"];

            foreach (var column in columnsForKey)
            {
                if (acceptData.Columns.Contains(column) == false)
                {
                    Console.WriteLine($"Missing column: {column}");
                    return;
                }
            }

            foreach (DataRow row in acceptData.Rows)
            {
                //if the row is missing a columnnnnn then it will be unable to make the unique string so I do not need to check for it seperatly
                try
                {
                    string uniqueKey = $"{row["created_at"]}-{row["order_name"]}-{row["product_line"]}-{row["product_colour"]}-{row["product_size"]}";

                    if((duplicateDictionary.ContainsKey(uniqueKey) == true))
                    {
                        duplicateRows.Add((row,"Is a duplicate row"));
                    }
                    else
                    {
                        duplicateDictionary[uniqueKey] = row;
                    }
                }
                catch(Exception e)
                {

                    string generatingIssue = "Issues when generating uniqueKey";
                    Console.WriteLine($"{generatingIssue} {e.Message}");
                    duplicateRows.Add((row, generatingIssue));
                }
            }


            //adds the duplicates to rejectData and removes them from acceptData
            foreach (var (row,reason) in duplicateRows)
            {
                SegregateIncorrectData(rejectData, row, reason);
            }

            foreach (var (row, reason) in duplicateRows)
            {
                acceptData.Rows.Remove(row);
            }

        }

        private static void MoveToRejectData(DataTable acceptData, DataTable rejectData,List<DataRow>invalidRows, string rejectReason)
        {
            //ensures that all the data inputted is valid
            if((acceptData == null)||(rejectData == null)||(invalidRows == null))
            {
                Console.WriteLine("Unable to transfer data as either acceptData, rejectData or invalidRows do not exist");
                return;

            }
            if (string.IsNullOrEmpty(rejectReason) == true)
            {
                rejectReason = "No rejectReason provided";
            }

            foreach(var row in invalidRows)
            {
                SegregateIncorrectData(rejectData, row, rejectReason);
                acceptData.Rows.Remove(row);
            }

        }
       
        //converts all the rejected data into a single string and adds to the first column in a reject list.
        //initially tried having another DataTable to store the overflow data but it could all be added to reject data as long as the first column was a string
        private static void HandleOverloadedRow(DataTable rejectList, IList<object> rowValues, string rejectReason)
        {
            try
            {
                var stringBuilder = new StringBuilder();

                //adds all the values from rowValues to a StringBuilder
                foreach (var value in rowValues)
                {
                    if ((value != null) && (string.IsNullOrWhiteSpace(value.ToString()) == false))
                    {
                        if (stringBuilder.Length > 0)
                        {
                            stringBuilder.Append(", ");
                        }
                        stringBuilder.Append(value.ToString());
                    }
                }

                var valuesToString = stringBuilder.ToString();


                DataRow rejectRow = rejectList.NewRow();
                rejectRow[0] = valuesToString;
                rejectRow[1] = rejectReason;
                rejectList.Rows.Add(rejectRow);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Issue procesing row {string.Join(",", rowValues)}. {e.Message}");
            }
        }
        private static void InsertIntoDataTable(DataTable acceptList, DataTable rejectList, IList<object> rowValues)
        {
            try
            {
                DataRow newRow = acceptList.NewRow();

                for (int i = 0; i < rowValues.Count; i++)
                {


                    if (acceptList.Columns[i].ColumnName == "sku")
                    {
                        string skuValue = rowValues[i]?.ToString().Trim();

                        if (string.IsNullOrEmpty(skuValue))
                        {
                            HandleOverloadedRow(rejectList, rowValues, " The SKU is missing or null.");
                            return;  
                        }

                        newRow[i] = skuValue;
                    }
                    //converts the usual Yes or No to boolean

                    else if (acceptList.Columns[i].ColumnName == "is_exchange")
                    {
                        //ensures that the string is formatted properly
                        string is_exchangeString = rowValues[i]?.ToString()?.Trim().ToUpper();
                        
                        if (is_exchangeString == "YES")
                        {
                            newRow[i] = true;
                        }
                        else if (is_exchangeString == "NO")
                        {
                            newRow[i] = false;
                        }
                        else
                        {
                            
                            HandleOverloadedRow(rejectList, rowValues, "The value inside 'is_exchange' is invalid");
                            return;
                        }
                    }
                    
                    else
                    {
                        //if the value if null, it uses the DBNull.Value instead of adding a blank

                        newRow[i] = rowValues[i] ?? DBNull.Value;
                    }
                }
 

                acceptList.Rows.Add(newRow);
            }
            catch(Exception e)
            {
                HandleOverloadedRow(rejectList, rowValues, "Error occured when adding data to acceptTable");

                Console.WriteLine($"Issue procesing row {string.Join(",", rowValues)}. {e.Message}");
               
            }
        }

        //attempts to split the SKU up into product_line, product_colour and product_size, if it fails it moves the data from acceptData to rejectData
        private static void SKUProcessing(DataTable acceptData, DataTable rejectData)
        {
            List<DataRow> rejectedSKU = new List<DataRow>();

            if ((acceptData.Columns.Contains("sku") == false))
            {

                foreach (DataRow row  in acceptData.Rows)
                {
                    //moves all the data from acceptData to reject as it is missing the sku column
                    SegregateIncorrectData(rejectData, row, "Unable to find the sku column in the dataTable");
                }
                //as all the data has been rejected,  acceptData can be cleared
                acceptData.Rows.Clear();

                return;
            }

            if (acceptData.Columns.Contains("product_line") == false)
            {
                acceptData.Columns.Add("product_line", typeof(string));
            }
            if (acceptData.Columns.Contains("product_colour") == false)
            {
                acceptData.Columns.Add("product_colour", typeof(string));
            }
            if (acceptData.Columns.Contains("product_size") == false)
            {
                acceptData.Columns.Add("product_size", typeof(string));
            }

            foreach (DataRow row in acceptData.Rows)
            {
                string skuString = row["sku"]?.ToString();

                if (string.IsNullOrEmpty(skuString) == false)
                {
                    //tried using a RegEx split but found it was slower
                    string[] skuParts = skuString.Split('-');

                    if (skuParts.Length == 3)
                    {
                        row["product_line"] = skuParts[0];
                        row["product_colour"] = skuParts[1];
                        row["product_size"] = skuParts[2];
                    }
                    else
                    {
                        Console.WriteLine($"The sku is incorrectly formatted {skuString}.");
                        rejectedSKU.Add(row);
                    }
                }
                else
                {
                    Console.WriteLine("SKU is missing");
                    rejectedSKU.Add(row);

                }
            }
            string rejectReason = $"The sku is incorrectly formatted";

            MoveToRejectData(acceptData, rejectData, rejectedSKU, rejectReason);

            //initially I deleted the sku but kept it as I have found that it serves very well as a product id
             //acceptData.Columns.Remove("sku");
        }
        //converts the data from the DataRow to a string and then calls HandleOverloadedRow
        private static void SegregateIncorrectData( DataTable rejectData,DataRow row, string rejectReason)
        {
            var rowToList = row.ItemArray.ToList();

            if((rowToList == null)||(rowToList.Count == 0))
            {
                Console.WriteLine("row is empty so unable to process and move to rejectData");
                return;
            }
            else
            {
                HandleOverloadedRow(rejectData, rowToList, rejectReason);

            }


        }

        //used to check what is inside each dataTable
        public static void DataChecker(DataTable acceptData, DataTable rejectData)
        {


            Console.WriteLine("Accepted data");
            foreach (DataRow row in acceptData.Rows)
            {
                foreach (DataColumn column in acceptData.Columns)
                {
                    Console.Write(row[column] + "\t");
                }
                Console.WriteLine();

            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();


            Console.WriteLine("Rejected data");
            foreach (DataRow row in rejectData.Rows)
            {
                Console.WriteLine($"Data: {row["RejectedData"]}, Reason: {row["RejectionReason"]}");
            }
            Console.WriteLine();
        }

        public static DataTable CreateAcceptTable(int numofColumns)
        {
            DataTable table = new DataTable();
            table.Columns.Add("created_at", typeof(DateTime));
            table.Columns.Add("order_name", typeof(int));
            table.Columns.Add("customer_name", typeof(string));
            table.Columns.Add("product_name", typeof(string));
            table.Columns.Add("sku", typeof(string));
            table.Columns.Add("return_reason", typeof(string));
            table.Columns.Add("return_note", typeof(string));
            table.Columns.Add("credit_method", typeof(string));
            table.Columns.Add("is_exchange", typeof(bool));
            


            int setColumns = 12;
            for (int i = setColumns; i < numofColumns; i++)
            {
                table.Columns.Add($"Additional data{i-setColumns}" , typeof(string));
            }
            
            
            return table;
        }

        public static DataTable CreateRejectTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("RejectedData", typeof(string));
            table.Columns.Add("RejectionReason", typeof(string));
            return table;
        }
    }
}
