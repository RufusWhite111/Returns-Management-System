using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NEAProjectMain.SQLSelectClasses;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace NEAProjectMain
{
    public partial class MainGUI : Form
    {
        // all the unique spreadsheet IDs but only use 1 and 2 as 3 since there was not much point wasting time filling it out

        private Dictionary<string, string> spreadsheetIDS = new Dictionary<string, string>
        {
            { "08/11/2023 to 02/07/2024", "YOUR_SHEET_ID_1" },
            { "01/01/2025 to 07/03/2025", "YOUR_SHEET_ID_2" },
            { "API Testing 3", "YOUR_SHEET_ID_3" }
        };

        private Label labelTitle;

        private Label labelSelectDatabase;
        private Label labelSelected;
        private Label labelPleaseWait;

        private Label labelSummary;
        private Label labelSummaryTitle;

        private Button buttonConfirm;


        private ComboBox spreadsheetSelector;


        private Button buttonQueryData;

        private Label labelMonthlyReturnsTitle;
        private PlotView monthlyReturnsBarChart;

        private Panel panelSummaryBorder;
        private Panel panelChartBorder;

        private Controller mainController;
        private QueryHandler queryHandler;


        //the constructor to initialise  MainGUI 
        public MainGUI(Controller controller)
        {
            mainController = controller;

            //this refers to the current instance of the class and is a reserved keyword in C# so I cannot rename it
            queryHandler = new QueryHandler(this, mainController);
            InitialiseComponent();
            this.Resize += new EventHandler(CentraliseObjects);
        }

        //sets up all the initial UI components
        private void InitialiseComponent()
        {
            this.Text = "Relational Database Management System";
            this.WindowState = FormWindowState.Maximized; //silly american spelling
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.BackColor = Color.White;

            //Label for the title of UI
            labelTitle = new Label();
            labelTitle.Text = "Relational Database Management System";
             labelTitle.Font = new Font("Arial", 24, FontStyle.Bold);
            labelTitle.TextAlign = ContentAlignment.MiddleCenter;
            labelTitle.AutoSize = false;
            labelTitle.Size = new Size(800, 50);
            this.Controls.Add(labelTitle);

            // Select database label
            labelSelectDatabase = new Label();
            labelSelectDatabase.Text = "Please select the database:";

            labelSelectDatabase.Font = new Font("Arial", 14);
            labelSelectDatabase.TextAlign = ContentAlignment.MiddleCenter;
            labelSelectDatabase.AutoSize = false;
            labelSelectDatabase.Size = new Size(400, 30);
            this.Controls.Add(labelSelectDatabase);

            // Spreadsheet selector drop down
            spreadsheetSelector = new ComboBox();
            spreadsheetSelector.Size = new Size(400, 40);
            spreadsheetSelector.Font = new Font("Arial", 14);
            spreadsheetSelector.DataSource = new BindingSource(spreadsheetIDS, null);
            spreadsheetSelector.DisplayMember = "Key";
            spreadsheetSelector.ValueMember = "Value";
            spreadsheetSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(spreadsheetSelector);

            // button for confirms the database
            buttonConfirm = new Button();
            buttonConfirm.Text = "Confirm";
            buttonConfirm.Font = new Font("Arial", 14, FontStyle.Bold);
            buttonConfirm.Size = new Size(150, 40);
            buttonConfirm.Click += new EventHandler(buttonConfirm_Click);
            this.Controls.Add(buttonConfirm);

            // Selected which database label
            labelSelected = new Label();
            labelSelected.Text = "";
            labelSelected.Font = new Font("Arial", 14);
            labelSelected.TextAlign = ContentAlignment.MiddleCenter;
            labelSelected.AutoSize = false;
            labelSelected.Size = new Size(400, 30);
            
            //like most other components is initially hidden
            labelSelected.Visible = false;
            this.Controls.Add(labelSelected);

            // Please Wait label
            labelPleaseWait = new Label();
            labelPleaseWait.Text = "Please Wait...";
            labelPleaseWait.Font = new Font("Arial", 14);
            labelPleaseWait.TextAlign = ContentAlignment.MiddleCenter;
            labelPleaseWait.AutoSize = false;
            labelPleaseWait.Size = new Size(400, 30);
            labelPleaseWait.Visible = false; 
            this.Controls.Add(labelPleaseWait);

            // Bar chart of monthly returns label
            labelMonthlyReturnsTitle = new Label();
            labelMonthlyReturnsTitle.Text = "Monthly Return Breakdown";
            labelMonthlyReturnsTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            labelMonthlyReturnsTitle.TextAlign = ContentAlignment.TopLeft;
            labelMonthlyReturnsTitle.AutoSize = true;
            labelMonthlyReturnsTitle.Size = new Size(400, 30);
            labelMonthlyReturnsTitle.Visible = false; 
            this.Controls.Add(labelMonthlyReturnsTitle);

            // Summary of database title
            labelSummaryTitle = new Label();
            labelSummaryTitle.Text = "Summary";
            labelSummaryTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            labelSummaryTitle.TextAlign = ContentAlignment.TopLeft;
            labelSummaryTitle.AutoSize = false;
            labelSummaryTitle.Size = new Size(300, 30);
            labelSummaryTitle.Visible = false; 
            this.Controls.Add(labelSummaryTitle);

            // Panel for Summary - used as a border
            panelSummaryBorder = new Panel();
            panelSummaryBorder.BorderStyle = BorderStyle.FixedSingle; 
            panelSummaryBorder.Size = new Size(580, 420);
            panelSummaryBorder.Visible = false;
            this.Controls.Add(panelSummaryBorder);

            // Summary data 
            labelSummary = new Label();
            labelSummary.Text = "";
            //adjusted from bold to Italic to make it stand out
            labelSummary.Font = new Font("Arial", 14, FontStyle.Italic); 
            labelSummary.TextAlign = ContentAlignment.TopLeft;
            labelSummary.AutoSize = false;
            labelSummary.Size = new Size(560, 400);
            labelSummary.Visible = false; 
            panelSummaryBorder.Controls.Add(labelSummary);

            //border for bar chart
            panelChartBorder = new Panel();
            panelChartBorder.BorderStyle = BorderStyle.FixedSingle;
            panelChartBorder.Size = new Size(580, 420);
            panelChartBorder.Visible = false; 
            this.Controls.Add(panelChartBorder);

            // Bar chart of monthly breakdown of returns
            monthlyReturnsBarChart = new PlotView();
            monthlyReturnsBarChart.Size = new Size(560, 400);
            monthlyReturnsBarChart.Visible = false; 
            panelChartBorder.Controls.Add(monthlyReturnsBarChart);

            // Switch to queries selection
            buttonQueryData = new Button();
            buttonQueryData.Text = "Query Data";
            buttonQueryData.Font = new Font("Arial", 14, FontStyle.Bold);
            buttonQueryData.Size = new Size(200, 40);
            buttonQueryData.Click += new EventHandler(buttonQueryData_Click);
            buttonQueryData.Visible = false; 
            this.Controls.Add(buttonQueryData);

            //is called as I want to have my UI consutructed symetrically
            CentraliseObjects(null, null);
        }

        //function which makes sure all objects are always centralised, even if the size of the windows changes
        private void CentraliseObjects(object sender, EventArgs eventData)
        {
            int centerOfWindowX = this.ClientSize.Width / 2;

            int spacing = 120; //have adjusted to have larger gaps between borders

            // calculates how much width I have to work with
            int totalWidth = panelChartBorder.Width + spacing + panelSummaryBorder.Width;

            labelTitle.Location = new Point(centerOfWindowX - (labelTitle.Width / 2), 20);
            labelSelectDatabase.Location = new Point(centerOfWindowX - (labelSelectDatabase.Width / 2), 100);

            spreadsheetSelector.Location = new Point(centerOfWindowX - (spreadsheetSelector.Width / 2), 150);

            buttonConfirm.Location = new Point(centerOfWindowX - (buttonConfirm.Width / 2), 210);


            labelSelected.Location = new Point(centerOfWindowX - (labelSelected.Width / 2), 260);
            labelPleaseWait.Location = new Point(centerOfWindowX - (labelPleaseWait.Width / 2), 300);

            // used as a variable so both titles are positioned at the right height
            int titleY = 350;

            // calculates start position to center the entire layout
            int startX = (this.ClientSize.Width - totalWidth) / 2;

            
            labelMonthlyReturnsTitle.Location = new Point(startX, titleY);
            panelChartBorder.Location = new Point(startX, titleY + 40);

            int summaryX = startX + panelChartBorder.Width + spacing;
            labelSummaryTitle.Location = new Point(summaryX, titleY);
            panelSummaryBorder.Location = new Point(summaryX, titleY + 40);

            //makes sure the plot is inside the border
            monthlyReturnsBarChart.Location = new Point(10, 10);

            // Position the summary text inside its border
            labelSummary.Location = new Point(10, 10);

            //ensures that the Query button is below both panels as I had issues with them overlapping
            int maxBottomY = Math.Max(panelChartBorder.Bottom, panelSummaryBorder.Bottom);
            buttonQueryData.Location = new Point(centerOfWindowX - (buttonQueryData.Width / 2), maxBottomY + 50);
        }

        private async void buttonConfirm_Click(object sender, EventArgs eventData)
        {
            if (spreadsheetSelector.SelectedItem != null)
            {
                string selectedName = ((KeyValuePair<string, string>)spreadsheetSelector.SelectedItem).Key;
                string selectedID = ((KeyValuePair<string, string>)spreadsheetSelector.SelectedItem).Value;



                // Used to immediately show "Selected" and "Please Wait" labels
                labelSelected.Text = $"Selected: {selectedName}";
                labelSelected.Visible = true;
                labelPleaseWait.Visible = true;

                buttonConfirm.Visible = false;

                // Forces the UI to update before processing as otherwise the user would be confused
                Application.DoEvents();

                try
                {
                    await ProcessDatabaseAsync(selectedID);

                    // Updates and shows all the summary labels
                    string summaryData = mainController.GetDatabaseSummary();
                    labelSummary.Text = summaryData;
                    labelSummary.Visible = true;
                    labelSummaryTitle.Visible = true;
                    panelSummaryBorder.Visible = true;

                    
                    var monthlyReturnCounts = mainController.GetMonthlyReturnCounts();
                    UpdateBarChart(monthlyReturnCounts);

                    // displays the bar chart and its title
                    labelMonthlyReturnsTitle.Visible = true;
                    panelChartBorder.Visible = true;
                    monthlyReturnsBarChart.Visible = true;

                    buttonQueryData.Visible = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error processing database: {e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    labelPleaseWait.Visible = false; // Hide's "Please Wait"  after processing as no need to have it
                }
            }
        }

        private async Task ProcessDatabaseAsync(string spreadsheetID)
        {
            await Task.Run(() => { mainController.ProcessSelectedDatabase(spreadsheetID); });
        }

        private void UpdateBarChart(List<MonthlyReturnCount> monthlyReturnCounts)
        {
            var monthlyReturnsPlotModel = new PlotModel { Title = "" };

            //BarSeries works by having a collection of BarItems, and each one represents a bar
            BarSeries monthlyReturnsBarSeries = new BarSeries
            {
                LabelPlacement = LabelPlacement.Outside,
                LabelFormatString = "{0}" 
            };

            string[] monthNames = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            CategoryAxis categoryAxis = new CategoryAxis { Position = AxisPosition.Left };

            // Sort the monthlyReturnCounts list in descending order by month using a merge sort
            // This ensures the bars are displayed in reverse chronological order as the dates
            // wrap around so it looks alot cleaner to sort it like this instead of going Nov, Dec straight to June
            monthlyReturnCounts = SplitListIntoTwoLists(monthlyReturnCounts);


            for (int i = 0; i < monthlyReturnCounts.Count; i++)
            {
                var monthlyReturn = monthlyReturnCounts[i];

                //this converts the number to its name
                int monthIndex = monthlyReturn.Month - 1;
                string monthName = (monthIndex >= 0 && monthIndex < 12) ? monthNames[monthIndex] : "Unknown"; //if out of range just puts UnKnown

                
                monthlyReturnsBarSeries.Items.Add(new BarItem { Value = monthlyReturn.ReturnCount });

                
                categoryAxis.Labels.Add(monthName);
            }

            
            monthlyReturnsPlotModel.Series.Add(monthlyReturnsBarSeries);

            
            monthlyReturnsPlotModel.Axes.Add(categoryAxis);

            
            monthlyReturnsPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom, 
                MinimumPadding = 0, // ensures that the Axis starts at 0
                MaximumPadding = 0.1,
                AbsoluteMinimum = 0 // Axis cannot display a negative plot
            });

            
            monthlyReturnsBarChart.Model = monthlyReturnsPlotModel;
        }

        public static List<MonthlyReturnCount> SplitListIntoTwoLists(List<MonthlyReturnCount> list) 
            //public so can be called from the GUI Query handler
        {
            //the base case
            if (list.Count <= 1)
            {
                return list;

            }

            //splits the list down into 2 halves
            int mid = list.Count / 2;

            List<MonthlyReturnCount> left = (list.GetRange(0, mid));
            List<MonthlyReturnCount> right = (list.GetRange(mid, list.Count - mid));
            
            //recursively sorts them both
            left = SplitListIntoTwoLists(left);
            right = SplitListIntoTwoLists(right);

            // Combines the 2 halves back together
            return MergeAndSortTwoLists(left, right);
        }

        private static List<MonthlyReturnCount> MergeAndSortTwoLists(List<MonthlyReturnCount> left, List<MonthlyReturnCount> right)
        {
            List<MonthlyReturnCount> result = new List<MonthlyReturnCount>();
            int leftIndex = 0;
            int rightIndex = 0;


            // Merge the two lists in descending order
            while (leftIndex < left.Count && rightIndex < right.Count)
            {
                if (left[leftIndex].Month >= right[rightIndex].Month)
                {

                    result.Add(left[leftIndex]);
                    leftIndex++;
                }
                else
                {
                    result.Add(right[rightIndex]);
                    rightIndex++;
                }
            }

            // Adds any of the remaining elements from the left list
            while (leftIndex < left.Count)
            {
                result.Add(left[leftIndex]);
                leftIndex++;
            }

            // same thing with the right
            while (rightIndex < right.Count)
            {
                result.Add(right[rightIndex]);
                rightIndex++;
            }

            return result;
        }

        // when the MakeQueries(current name for now) is clicked, used to hide all the visible components
        private void buttonQueryData_Click(object sender, EventArgs eventData)
        {
            labelTitle.Visible = false;
            labelSelectDatabase.Visible = false;
            spreadsheetSelector.Visible = false;
            buttonConfirm.Visible = false;
            labelSelected.Visible = false;
            labelPleaseWait.Visible = false;
            labelSummary.Visible = false;
            labelSummaryTitle.Visible = false;
            labelMonthlyReturnsTitle.Visible = false;
            monthlyReturnsBarChart.Visible = false;
            buttonQueryData.Visible = false;
            panelSummaryBorder.Visible = false;
            panelChartBorder.Visible = false;
            
            //This method hides all current UI elements and initialises and displays the query selection interface
            queryHandler.ShowQuerySelectionPage();
        }
    }
}