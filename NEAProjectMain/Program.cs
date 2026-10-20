using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Net;
using System.Data.SqlClient;
using System.Windows.Forms;


namespace NEAProjectMain

{
    public static class Program
    {
        //apparently I have to use STAThread when using WinForms because the UI has to run in a "Single Threaded Apartment" Mode:
        //https://learn.microsoft.com/en-us/dotnet/api/system.stathreadattribute?view=net-9.0
        [STAThread]


        //used to disable automatic DPI scaling functions as before all the text in my GUI was blurry
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        public static void Main()
        {
            string sheetID1 = "YOUR_SHEET_ID_1";
            string sheetID2 = "YOUR_SHEET_ID_2";
            string sheetID3 = "YOUR_SHEET_ID_3";

            //ConnectingToSQL.SQLConnection(sheetID1);

            GUIImplementation();
            //ConnectingToSQL.SQLConnection(sheetID1);

        }

        

        public static void GUIImplementation()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            //Checks if  OS system is being used and stops the application scaling automatically
            if (Environment.OSVersion.Version.Major >= 6) 
            {
                SetProcessDPIAware(); 
            }

            Controller controller = new Controller();

            MainGUI mainForm = new MainGUI(controller);

            Application.Run(mainForm);
        }
   
    }
}

