using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Drawing;
using NEAProjectMain.SQLSelectClasses;
using System.Data;

namespace NEAProjectMain
{

    // I broke the GUI up into this and the Main as otherwise the clss was getting too big
    public class QueryHandler
    {
        //references the main "form" of the application which actually displays the UI

        private Form mainForm;

        //acts as a joiner/bridge between the UI and the database
        private Controller dataController;

        //stores all the UI controls
        private List<Control> queryUIControls = new List<Control>();

        public QueryHandler(Form parentForm, Controller controller)
        {
            mainForm = parentForm;
            dataController = controller;
        }
        public void ShowQuerySelectionPage()
        {
            // removes any existing objects for the UI as I have not implemented a back button
            mainForm.Controls.Clear();
            queryUIControls.Clear();

            // the title for the query selection page
            var labelQueryTitle = new Label();
            labelQueryTitle.Text = "Select a Query";
            labelQueryTitle.Font = new Font("Arial", 24, FontStyle.Bold);
            labelQueryTitle.TextAlign = ContentAlignment.MiddleCenter;

            labelQueryTitle.AutoSize = false;
            labelQueryTitle.Size = new Size(800, 50);
            labelQueryTitle.Location = new Point((mainForm.ClientSize.Width - labelQueryTitle.Width) / 2, 20);


            mainForm.Controls.Add(labelQueryTitle);
            queryUIControls.Add(labelQueryTitle);

            // the drop-down menu where the user selects which specific query they want to form
            var queriesSelector = new ComboBox();
            queriesSelector.Size = new Size(400, 40);
            queriesSelector.Font = new Font("Arial", 14);
            queriesSelector.Items.AddRange(new string[]
            {
                 "Exchange vs Return Counts",
                   "Returns by Month",
                "Rank Return Items by Characteristic",
                "Rank Return Reasons",
                "Get Order Details by ID",
                "Get Customer Orders by Name",
                "Get Num of Orders Between Dates",
                "View Rejected Data"


            });

            //Implemented to stop the user from typing in the ComboBox
            queriesSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            queriesSelector.Location = new Point((mainForm.ClientSize.Width - queriesSelector.Width) / 2, 100);
            mainForm.Controls.Add(queriesSelector);
            queryUIControls.Add(queriesSelector);


            // clicks to confirm the query selected
            var buttonConfirmQuery = new Button();
            buttonConfirmQuery.Text = "Confirm Query";
            buttonConfirmQuery.Font = new Font("Arial", 14, FontStyle.Bold);

            buttonConfirmQuery.Size = new Size(200, 40);
            buttonConfirmQuery.Location = new Point((mainForm.ClientSize.Width - buttonConfirmQuery.Width) / 2, 160);
            buttonConfirmQuery.Click += (s, ev) => HandleQuerySelection(queriesSelector.SelectedItem?.ToString());

            mainForm.Controls.Add(buttonConfirmQuery);
            queryUIControls.Add(buttonConfirmQuery);
        }


        private void HandleQuerySelection(string selectedQuery)
        {
            if (string.IsNullOrEmpty(selectedQuery))
            {


                //if all is good displays the first string, if there is an issue displays "erroe"
                MessageBox.Show("Please select a query.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            mainForm.Controls.Clear();
            queryUIControls.Clear();

            //implemented a back button to allow the user to go back and select a different query if they want
            var buttonBack = new Button();

            buttonBack.Text = "Back to Queries";
            buttonBack.Font = new Font("Arial", 12, FontStyle.Bold);
            buttonBack.Size = new Size(180, 30); // Increased width from 150 to 180 to fit with my previous style
            buttonBack.Location = new Point(20, 20);
            buttonBack.Click += (s, e) => ShowQuerySelectionPage();

            mainForm.Controls.Add(buttonBack);
            queryUIControls.Add(buttonBack);

            var labelQueryTitle = new Label();
            labelQueryTitle.Font = new Font("Arial", 20, FontStyle.Bold);
            labelQueryTitle.TextAlign = ContentAlignment.MiddleCenter;
            labelQueryTitle.AutoSize = false;
            labelQueryTitle.Size = new Size(800, 40);
            labelQueryTitle.Location = new Point((mainForm.ClientSize.Width - labelQueryTitle.Width) / 2, 20);
            mainForm.Controls.Add(labelQueryTitle);
            queryUIControls.Add(labelQueryTitle);


            //as in the mainGUI, used a case instead of a switch as it is much faster and I think cleaner
            switch (selectedQuery)
            {
                case "Returns by Month":
                    labelQueryTitle.Text = "Returns by Month";
                    ShowReturnsByMonth();
                    break;

                case "Exchange vs Return Counts":
                    labelQueryTitle.Text = "Exchange vs Return Counts";
                    ShowExchangeVsReturnCounts();
                    break;

                case "Rank Return Items by Characteristic":
                    labelQueryTitle.Text = "Rank Return Items by Characteristic";
                    ShowRankItemsForm();
                    break;

                case "Rank Return Reasons":
                    labelQueryTitle.Text = "Rank Return Reasons";
                    ExecuteRankReturnReasonsQuery();
                    break;

                case "Get Order Details by ID":
                    labelQueryTitle.Text = "Get Order Details by ID";
                    ShowOrderDetailsForm();
                    break;

                case "Get Customer Orders by Name":
                    labelQueryTitle.Text = "Get Customer Orders by Name";
                    ShowCustomerOrdersForm();
                    break;

                case "Get Num of Orders Between Dates":
                    labelQueryTitle.Text = "Get Orders Between Dates";
                    ShowDateRangeQueryForm();
                    break;

                case "View Rejected Data":
                    labelQueryTitle.Text = "Rejected Data";
                    ShowRejectedData();
                    break;

                default:
                    MessageBox.Show("Invalid query selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ShowQuerySelectionPage();
                    break;
            }
        }

        //returns the amount of each and then the percentage ration - the higher the better
        private void ShowExchangeVsReturnCounts()
        {
            try
            {
                var exchangeVsReturnCounts = dataController.GetExchangeVsReturnCounts();

                var resultsPanel = new Panel();
                resultsPanel.BorderStyle = BorderStyle.FixedSingle;
                resultsPanel.Size = new Size(500, 300);
                resultsPanel.Location = new Point((mainForm.ClientSize.Width - resultsPanel.Width) / 2, 120);

                mainForm.Controls.Add(resultsPanel);
                queryUIControls.Add(resultsPanel);

                Label labelExchangeResults = new Label();
                var sb = new StringBuilder();


                sb.AppendLine();

                //adds all the data to the string builder
                foreach (var item in exchangeVsReturnCounts)
                {
                    sb.AppendLine($"Number of Exchanges: {item.NumofExchanges}");
                    sb.AppendLine($"Number of Returns: {item.NumofReturns}");
                    // returns the issue if trying to divide by 0  as would throw an error
                    sb.AppendLine($"Exchange to Return Ratio: {(item.NumofReturns > 0 ? 
                        ((double)item.NumofExchanges / item.NumofReturns).ToString("P2") : "N/A")}"); 
                }

                
                labelExchangeResults.Text = sb.ToString();
                labelExchangeResults.Font = new Font("Arial", 14);

                labelExchangeResults.TextAlign = ContentAlignment.TopLeft;
                labelExchangeResults.AutoSize = false;
                labelExchangeResults.Size = new Size(480, 280);
                labelExchangeResults.Location = new Point(10, 10);

                resultsPanel.Controls.Add(labelExchangeResults);
            }
            catch (Exception e)
            {

                MessageBox.Show($"There was anerror retrieving exchange vs return counts: {e.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ShowReturnsByMonth()
        {
            try
            {

                // Fetches the monthly returns data from the controller so I don't need to pass the connection in
                var monthlyReturnCounts = dataController.GetMonthlyReturnCounts();

                var resultsPanel = new Panel();
                resultsPanel.BorderStyle = BorderStyle.FixedSingle;
                resultsPanel.AutoScroll = true;
                resultsPanel.Size = new Size(600, 500);
                resultsPanel.Location = new Point((mainForm.ClientSize.Width - resultsPanel.Width) / 2, 120);
                mainForm.Controls.Add(resultsPanel);
                queryUIControls.Add(resultsPanel);


                string[] monthNames = {"January", "February", "March", "April", "May", "June","July", 
                    "August", "September", "October", "November", "December"};


                // Sorts the list in descending order by month (latest month first)
                monthlyReturnCounts = MainGUI.SplitListIntoTwoLists(monthlyReturnCounts);

                var labelResults = new Label();
                var sb = new StringBuilder();

                sb.AppendLine("The monthly break down is:");
                sb.AppendLine();

                foreach (var monthlyReturn in monthlyReturnCounts)
                {
                    int monthIndex = monthlyReturn.Month - 1;

                    //if out of range still returms the data but just specifies unknown. Doubt this will happen though
                    string monthName = (monthIndex >= 0 && monthIndex < 12) ? monthNames[monthIndex] : "Unknown";

                    sb.AppendLine($"{monthName}: {monthlyReturn.ReturnCount} returns");
                }

                // Displays the monthy return data
                labelResults.Text = sb.ToString();
                labelResults.Font = new Font("Arial", 14);
                labelResults.TextAlign = ContentAlignment.TopLeft;
                labelResults.AutoSize = true;
                labelResults.Location = new Point(10, 10);
                resultsPanel.Controls.Add(labelResults);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error retrieving returns by month: {e.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ShowDateRangeQueryForm()
        {

            //need to remove any existing objects
            RemoveResultPanels();



            //tells the user to choose which dates they want
            Label labelDateRange = new Label();
            labelDateRange.Text = "Select Date Range:";
            labelDateRange.Font = new Font("Arial", 14);
            labelDateRange.TextAlign = ContentAlignment.MiddleCenter;
            labelDateRange.AutoSize = false;
            labelDateRange.Size = new Size(400, 30);
            labelDateRange.Location = new Point((mainForm.ClientSize.Width - labelDateRange.Width) / 2, 80);

            mainForm.Controls.Add(labelDateRange);
            queryUIControls.Add(labelDateRange);

            //for getting the user to choose which start date they want
            var labelStartDate = new Label();
            labelStartDate.Text = "Start Date:";
            labelStartDate.Font = new Font("Arial", 14);
            labelStartDate.AutoSize = true;
            labelStartDate.Location = new Point((mainForm.ClientSize.Width / 2) - 250, 120);
            mainForm.Controls.Add(labelStartDate);
            queryUIControls.Add(labelStartDate);

            var startDateSelection = new DateTimePicker();
            startDateSelection.Format = DateTimePickerFormat.Short;
            startDateSelection.Size = new Size(200, 30);
            startDateSelection.Font = new Font("Arial", 14);
            startDateSelection.Location = new Point((mainForm.ClientSize.Width / 2) - 250, 150);
            mainForm.Controls.Add(startDateSelection);
            queryUIControls.Add(startDateSelection);

            //user picking end data
            var labelEndDate = new Label();
            labelEndDate.Text = "End Date:";
            labelEndDate.Font = new Font("Arial", 14);
            labelEndDate.AutoSize = true;
            labelEndDate.Location = new Point((mainForm.ClientSize.Width / 2) + 50, 120);
            mainForm.Controls.Add(labelEndDate);
            queryUIControls.Add(labelEndDate);

            var endDateSelection = new DateTimePicker();
            endDateSelection.Format = DateTimePickerFormat.Short;
            endDateSelection.Size = new Size(200, 30);
            endDateSelection.Font = new Font("Arial", 14);
            endDateSelection.Location = new Point((mainForm.ClientSize.Width / 2) + 50, 150);
            mainForm.Controls.Add(endDateSelection);
            queryUIControls.Add(endDateSelection);


            //creates the button where the user can submit the dates they selected
            var buttonExecuteDateQuery = new Button();
            buttonExecuteDateQuery.Text = "Execute Query";
            buttonExecuteDateQuery.Font = new Font("Arial", 14, FontStyle.Bold);
            buttonExecuteDateQuery.Size = new Size(200, 40);
            buttonExecuteDateQuery.Location = new Point((mainForm.ClientSize.Width - 
                buttonExecuteDateQuery.Width) / 2, 200);
            buttonExecuteDateQuery.Click += (s, e) => 
                ExecuteDateRangeQuery(startDateSelection.Value, endDateSelection.Value);

            mainForm.Controls.Add(buttonExecuteDateQuery);
            queryUIControls.Add(buttonExecuteDateQuery);
        }

        private void ExecuteDateRangeQuery(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Gets the number of orders between the two dates
                int orderCount = dataController.GetOrderCountBetweenDates(startDate, endDate);

                var resultsPanel = new Panel();
                resultsPanel.BorderStyle = BorderStyle.FixedSingle;
                resultsPanel.Size = new Size(450, 200); // made slightly smaller as less data being displayed
                resultsPanel.Location = new Point((mainForm.ClientSize.Width - resultsPanel.Width) / 2, 220);
                mainForm.Controls.Add(resultsPanel);
                queryUIControls.Add(resultsPanel);



                var labelResults = new Label();
                //used ToShortDateString as don't need to display data and time to the exact millisecond
                labelResults.Text = $"Number of orders between {startDate.ToShortDateString()} and " +
                    $"{endDate.ToShortDateString()}: {orderCount}";
                labelResults.Font = new Font("Arial", 14);
                labelResults.TextAlign = ContentAlignment.MiddleCenter;
                labelResults.AutoSize = false;
                labelResults.Size = new Size(480, 180);
                labelResults.Location = new Point(10, 10);

                resultsPanel.Controls.Add(labelResults);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error executing query: {e.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowRankItemsForm()
        {

            //am displaying another query so don't want any other information on the screen
            RemoveResultPanels();

            //title for the combobox selector
            var labelCharacteristicSelector = new Label();
            labelCharacteristicSelector.Text = "Select Characteristic to Rank By:";
            labelCharacteristicSelector.Font = new Font("Arial", 14);
            labelCharacteristicSelector.TextAlign = ContentAlignment.MiddleCenter;
            labelCharacteristicSelector.AutoSize = false;
            labelCharacteristicSelector.Size = new Size(400, 30);
            labelCharacteristicSelector.Location = new Point((mainForm.ClientSize.Width - 
                labelCharacteristicSelector.Width) / 2, 100);

            mainForm.Controls.Add(labelCharacteristicSelector);
            queryUIControls.Add(labelCharacteristicSelector);

            var characteristicsList = new string[]
            {
                "ProductName","ProductColour", "ProductSize",
            };

            //DropDown selector for user to pick how they want to sort the data
            var characteristicsSelector = new ComboBox();
            characteristicsSelector.Size = new Size(300, 30);
            characteristicsSelector.Font = new Font("Arial", 14);

            characteristicsSelector.Items.AddRange(characteristicsList);
            characteristicsSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            // adjusted to position further below label
            characteristicsSelector.Location = new Point((mainForm.ClientSize.Width - 
                characteristicsSelector.Width) / 2, 140); 

            mainForm.Controls.Add(characteristicsSelector);
            queryUIControls.Add(characteristicsSelector);


            
            var buttonExecute = new Button();
            buttonExecute.Text = "Execute";
            buttonExecute.Font = new Font("Arial", 14, FontStyle.Bold);
            buttonExecute.Size = new Size(200, 40);

            //when clicked runs the ExecuteRankItems but inserting which characteristic they chose
            buttonExecute.Location = new Point((mainForm.ClientSize.Width - buttonExecute.Width) / 2, 190); 
            buttonExecute.Click += (s, e) => 
                ExecuteRankItemsQuery(characteristicsSelector.SelectedItem?.ToString());
            mainForm.Controls.Add(buttonExecute);
            queryUIControls.Add(buttonExecute);
        }


        private void ExecuteRankItemsQuery(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                MessageBox.Show("Please select a characteristic to filter.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                
                var rankedItems = dataController.GetRankedItems(columnName);


                var resultsPanel = new Panel();
                resultsPanel.BorderStyle = BorderStyle.FixedSingle;
                resultsPanel.AutoScroll = true;
                resultsPanel.Size = new Size(600, 400);
                resultsPanel.Location = new Point((mainForm.ClientSize.Width - resultsPanel.Width) / 2, 240);
                mainForm.Controls.Add(resultsPanel);
                queryUIControls.Add(resultsPanel);

                var labelResults = new Label();
                var sb = new StringBuilder();

                sb.AppendLine($"Ranked {columnName} by Return Count:");
                sb.AppendLine();

                //adds each item, its rank and the number of it returned to the string builder
                foreach (var item in rankedItems)
                {

                    sb.AppendLine($"Rank {item.Rank}: {item.Item} - {item.ReturnCount} returns");
                }



                labelResults.Text = sb.ToString();
                labelResults.Font = new Font("Arial", 12);
                labelResults.TextAlign = ContentAlignment.TopLeft;
                labelResults.AutoSize = true;
                labelResults.Location = new Point(10, 10);

                resultsPanel.Controls.Add(labelResults);

            }
            catch (Exception e)
            {
                MessageBox.Show($"Error executing query: {e.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);


            }
        }


        //similar to function above but gets the reason for the returns instead of which items were returned,
        //decided to pretty much resuse the code
        private void ExecuteRankReturnReasonsQuery()
        {
            try
            {
                var rankedReasons = dataController.GetRankedReturnReasons();

                var resultsPanel = new Panel();
                resultsPanel.BorderStyle = BorderStyle.FixedSingle;
                resultsPanel.AutoScroll = true;
                resultsPanel.Size = new Size(600, 400);
                resultsPanel.Location = new Point((mainForm.ClientSize.Width - resultsPanel.Width) / 2, 120);

                mainForm.Controls.Add(resultsPanel);
                queryUIControls.Add(resultsPanel);

                var labelResults = new Label();
                var sb = new StringBuilder();




                sb.AppendLine();

                foreach (var reason in rankedReasons)
                {
                    sb.AppendLine($"Rank {reason.Rank}: {reason.ReturnReason} - {reason.ReturnCount} returns");
                }

                labelResults.Text = sb.ToString();
                labelResults.Font = new Font("Arial", 14);
                labelResults.TextAlign = ContentAlignment.TopLeft;
                labelResults.AutoSize = true;
                labelResults.Location = new Point(10, 10);
                resultsPanel.Controls.Add(labelResults);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error executing query: {e.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ShowOrderDetailsForm()
        {


            RemoveResultPanels();

            var labelOrderIdRequest = new Label();
            labelOrderIdRequest.Text = "Enter Order ID:";
            labelOrderIdRequest.Font = new Font("Arial", 14);

            labelOrderIdRequest.TextAlign = ContentAlignment.MiddleCenter;
            labelOrderIdRequest.AutoSize = false;
            labelOrderIdRequest.Size = new Size(400, 30);
            labelOrderIdRequest.Location = new Point((mainForm.ClientSize.Width - 
                labelOrderIdRequest.Width) / 2, 100);

             mainForm.Controls.Add(labelOrderIdRequest);
             queryUIControls.Add(labelOrderIdRequest);


            //textbox for user to enter the order ID
            var textBoxOrderId = new TextBox();
            textBoxOrderId.Size = new Size(300, 30);
            textBoxOrderId.Font = new Font("Arial", 14);
            textBoxOrderId.Location = new Point((mainForm.ClientSize.Width - 
                textBoxOrderId.Width) / 2, 140); 

            mainForm.Controls.Add(textBoxOrderId);
            queryUIControls.Add(textBoxOrderId);

            var buttonTextBoxExecute = new Button();
            buttonTextBoxExecute.Text = "Get Order Details";
            buttonTextBoxExecute.Font = new Font("Arial", 14, FontStyle.Bold);
            buttonTextBoxExecute.Size = new Size(200, 40);
            buttonTextBoxExecute.Location = new Point((mainForm.ClientSize.Width - 
                buttonTextBoxExecute.Width) / 2, 190);


            buttonTextBoxExecute.Click += (s, e) =>

            {
                if (int.TryParse(textBoxOrderId.Text, out int orderId) == true)
                {
                    ExecuteGetOrderDetailsQuery(orderId);
                }
                else
                {
                    MessageBox.Show("Please enter a valid Order ID.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            };


            mainForm.Controls.Add(buttonTextBoxExecute);
            queryUIControls.Add(buttonTextBoxExecute);
        }

        private void ExecuteGetOrderDetailsQuery(int orderId)
        {
            try
            {
                var orderDetails = dataController.GetOrderDetails(orderId);


                //displays the results from the orderDetails
                var resultsPanel = new Panel();
                resultsPanel.BorderStyle = BorderStyle.FixedSingle;
                resultsPanel.AutoScroll = true;
                resultsPanel.Size = new Size(700, 400);
                resultsPanel.Location = new Point((mainForm.ClientSize.Width - 
                    resultsPanel.Width) / 2, 240);

                mainForm.Controls.Add(resultsPanel);
                queryUIControls.Add(resultsPanel);
                

                //only needs to put in the customer details once as they are the same across both items in the order
                var labelResults = new Label();
                StringBuilder sb = new StringBuilder();


                sb.AppendLine($"Order Details for Order ID: {orderId}");
                sb.AppendLine();
                sb.AppendLine($"Customer Name: {orderDetails.CustomerName}");
                sb.AppendLine($"Created At: {orderDetails.CreatedAt}");
                sb.AppendLine($"Original Payment Method: {orderDetails.OriginalPaymentMethod}");
                sb.AppendLine();
                sb.AppendLine("Items in Order:");

                //iterates through for each item in the order

                foreach (var item in orderDetails.Items)
                {
                    sb.AppendLine($"- {item.ProductName}, {item.ProductColour}, Size: {item.ProductSize}");
                    sb.AppendLine($"  Return Reason: {item.ReturnReason}");


                    if (item.IsExchange == true)
                    {
                        sb.AppendLine("  Is Exchange: Yes");
                    }
                    else
                    {
                        sb.AppendLine("  Is Exchange: No");
                    }
                    sb.AppendLine();
                }

                labelResults.Text = sb.ToString();
                labelResults.Font = new Font("Arial", 12);
                labelResults.TextAlign = ContentAlignment.TopLeft;
                labelResults.AutoSize = true; // autosize to adjusts for amount of data
                labelResults.Location = new Point(10, 10);

                resultsPanel.Controls.Add(labelResults);
            }


            catch (Exception e)
            {
                MessageBox.Show($"Error executing query: {e.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowCustomerOrdersForm()
        {
            RemoveResultPanels();

            // Add title label for the customer orders form
            var labelEnterCustomerName = new Label();
            labelEnterCustomerName.Text = "Enter Customer Name (FirstName Surname):";
            labelEnterCustomerName.Font = new Font("Arial", 14);
            labelEnterCustomerName.TextAlign = ContentAlignment.MiddleCenter;
            labelEnterCustomerName.AutoSize = false;
            labelEnterCustomerName.Size = new Size(500, 30);
            labelEnterCustomerName.Location = new Point((mainForm.ClientSize.Width - 
                labelEnterCustomerName.Width) / 2, 100);

            mainForm.Controls.Add(labelEnterCustomerName);
            queryUIControls.Add(labelEnterCustomerName);



            var textBoxEnterCustomerName = new TextBox();
            textBoxEnterCustomerName.Size = new Size(400, 30);
            textBoxEnterCustomerName.Font = new Font("Arial", 14);
            textBoxEnterCustomerName.Location = new Point((mainForm.ClientSize.Width - 
                textBoxEnterCustomerName.Width) / 2, 140);

            mainForm.Controls.Add(textBoxEnterCustomerName);
            queryUIControls.Add(textBoxEnterCustomerName);

             var buttonExecute = new Button();
            buttonExecute.Text = "Get Customer Orders";
            buttonExecute.Font = new Font("Arial", 14, FontStyle.Bold);
            buttonExecute.Size = new Size(200, 40);
            buttonExecute.Location = new Point((mainForm.ClientSize.Width - 
                buttonExecute.Width) / 2, 190);


            buttonExecute.Click += (s, e) => 
                ExecuteGetCustomerOrdersQuery(textBoxEnterCustomerName.Text);
            mainForm.Controls.Add(buttonExecute);
            queryUIControls.Add(buttonExecute);
        }

        //simple summary of all the orders the user has.
        //Can then input the order id for the full details

        private void ExecuteGetCustomerOrdersQuery(string customerName)
        {
            try
            {
                 var customerOrders = dataController.GetCustomerOrders(customerName);



                var resultsPanel = new Panel();
                resultsPanel.BorderStyle = BorderStyle.FixedSingle;
                resultsPanel.AutoScroll = true;
                resultsPanel.Size = new Size(600, 400);
                resultsPanel.Location = new Point((mainForm.ClientSize.Width - 
                    resultsPanel.Width) / 2, 240);

                mainForm.Controls.Add(resultsPanel);
                queryUIControls.Add(resultsPanel);


                var labelResults = new Label();
                var sb = new StringBuilder();

                sb.AppendLine($"Orders for Customer: {customerName}");
                sb.AppendLine();
                sb.AppendLine($"Total Orders: {customerOrders.TotalOrders}");
                sb.AppendLine();



                if (customerOrders.TotalOrders > 0)
                {
                    sb.AppendLine("Order List:");
                    foreach (var order in customerOrders.Orders)
                    {
                        sb.AppendLine($"- Order ID: {order.OrderId}, Created At: {order.CreatedAt}");
                    }
                }
                else
                {
                    sb.AppendLine("No orders found for this customer.");
                }


                labelResults.Text = sb.ToString();
                labelResults.Font = new Font("Arial", 12);
                labelResults.TextAlign = ContentAlignment.TopLeft;
                labelResults.AutoSize = true;
                labelResults.Location = new Point(10, 10);


                resultsPanel.Controls.Add(labelResults);
            }
            catch (Exception e)
            {

                MessageBox.Show($"Error executing query: {e.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowRejectedData()
        {
            try
            {
                var rejectedData = dataController.GetRejectedData();

                var labelRejectedDataTitle = new Label();
                labelRejectedDataTitle.Text = "Rejected Data Overview";
                labelRejectedDataTitle.Font = new Font("Arial", 14, FontStyle.Bold);
                labelRejectedDataTitle.TextAlign = ContentAlignment.MiddleCenter;
                labelRejectedDataTitle.AutoSize = false;
                labelRejectedDataTitle.Size = new Size(800, 30); //made larger for longer title
                labelRejectedDataTitle.Location = new Point((mainForm.ClientSize.Width - 
                    labelRejectedDataTitle.Width) / 2, 20);


                mainForm.Controls.Add(labelRejectedDataTitle);
                queryUIControls.Add(labelRejectedDataTitle);

                var labelExplanation = new Label();
                labelExplanation.Text = "Below is all the erroneous data which needs to be corrected.";
                labelExplanation.Font = new Font("Arial", 12);
                labelExplanation.TextAlign = ContentAlignment.MiddleCenter; 
                labelExplanation.AutoSize = false;
                // Set fixed width as autosize was cutting off part of the text
                labelExplanation.Size = new Size(mainForm.ClientSize.Width - 40, 30); 
                labelExplanation.Location = new Point((mainForm.ClientSize.Width - 
                    labelExplanation.Width) / 2, 70);

                mainForm.Controls.Add(labelExplanation);
                queryUIControls.Add(labelExplanation);



                //used a dataGrid as worked nicely to display the columns of data
                DataGridView rejectedDataDisplay = new DataGridView();
                rejectedDataDisplay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                rejectedDataDisplay.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                rejectedDataDisplay.AllowUserToAddRows = false;
                rejectedDataDisplay.ReadOnly = true;
                rejectedDataDisplay.DataSource = rejectedData;
                rejectedDataDisplay.Font = new Font("Arial", 12);


                // Calculates dynamic size for the DataGridView as the amount of
                // data changes quite a lot between different arrays of data

                int rowHeight = rejectedDataDisplay.RowTemplate.Height;
                int headerHeight = rejectedDataDisplay.ColumnHeadersHeight;


                //used to determine the number of rows in the rejected data

                int rowCount = 0;


                if (rejectedData is DataTable dataTable)
                {
                    rowCount = dataTable.Rows.Count;
                }

                else if (rejectedData is IEnumerable<object> enumerable)
                {
                    rowCount = enumerable.Count();
                }

                else
                {
                    throw new InvalidOperationException("Unsupported data type for rejectedData.");
                }


                //Calculates the total height
                int totalHeight = (rowCount * rowHeight) + headerHeight + 10;

                // Reduced from 200 to -500 as was having a lot of grey spots as the bottom
                int maxHeight = mainForm.ClientSize.Height - 500;
                int dynamicHeight = Math.Min(totalHeight, maxHeight);

                rejectedDataDisplay.Size = new Size(mainForm.ClientSize.Width - 
                    40, dynamicHeight); 

                rejectedDataDisplay.Location = new Point((mainForm.ClientSize.Width 
                    - rejectedDataDisplay.Width) / 2, 120);

                mainForm.Controls.Add(rejectedDataDisplay);
                queryUIControls.Add(rejectedDataDisplay);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error retrieving rejected data: {e.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        ///used to remove all the objects in the Panels when I want to switch to a new page
        private void RemoveResultPanels()
        {
            List<Control> objectsToRemove = new List<Control>();

            //finds all the panels with a fixes Border and adds them to the list to remove.
            //I did this because I used to have a border with an adjustable size
            foreach (Control item in mainForm.Controls)
            {

                if (item is Panel panel && panel.BorderStyle == BorderStyle.FixedSingle)
                {

                      objectsToRemove.Add(panel);
                }
            }

            foreach (var item in objectsToRemove)
            {
                queryUIControls.Remove(item);

                mainForm.Controls.Remove(item);

                item.Dispose();
            }
        }

        //saves RAM by getting rid of any unwanted objects
        public void Dispose()
        {
            foreach (var item in queryUIControls)
            {
                if ((item.IsDisposed == false))
                {
                    item.Dispose();
                }
            }
            queryUIControls.Clear();
        }
    }
}
