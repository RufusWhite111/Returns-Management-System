using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace NEAProjectMain
{
    public class ReadingAPI
    {
        //used the guide from https://developers.google.com/sheets for help on OAuth2.0
        //used the design framework from https://developers.google.com/sheets/api/quickstart/python?authuser=0 to get going for structure
        public static ValueRange? APICall(string SheetID)
        {
            //gets credentialsFilePath from Json
            var credentialsFilePathJSON = new ConfigurationBuilder().AddJsonFile("SettingsProgram.json", optional: false).Build(); //throws an exception if the file isnt found


            //stores the file path as a string
            //(reloadOnChange: true) might be needed
            string credentialsFilePath = credentialsFilePathJSON["FilePaths:CredentialFilePath"] ?? string.Empty;



            //initalises variable for OAuth 2.0 verification
            GoogleCredential credential = GetValidToken(credentialsFilePath, SheetID);

            //checks to see if GetValidToken has worked
            if (credential == null)
            {
                Console.WriteLine("Failed to Authenticate");
                return null;
            }

            var service = OAuthAuthenticate(credential);

            try
            {
                //implemented the Sheet1 method for range as I will not need to check the size of the sheet first
                var request = service.Spreadsheets.Values.Get(SheetID, "Sheet1");
                ValueRange response = request.Execute();

                if (response.Values != null)
                {
                    return response;

                }
                else
                {
                    Console.WriteLine("No data found.");
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error accessing the Google Sheets API: {e.Message}");
                return null;
            }


        }
        //checks if the file containing the API key exists and if so reads the file 
        private static GoogleCredential? GetValidToken(string credentialsFilePath, string sheetID)
        {
            if (File.Exists(credentialsFilePath) == false)
            {
                Console.WriteLine("The file path for credentials is not found. Please check file path");
                return null;
            }

            string credentialsSaved = File.ReadAllText(credentialsFilePath);

           

            try
            {
                //creates a scoped credential
                var credentials = GoogleCredential.FromJson(credentialsSaved)
                    .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

                //checks to see if token validation has failed and attempts to regenerate token
                if (IsCredentialValid(credentials, sheetID) == false)
                {

                    //call the separate method to generate a new credential
                    credentials = GenerateNewCredential(credentialsSaved);

                    //checks again if regeneration has failed and if so returns null
                    if (IsCredentialValid(credentials, sheetID) == false)
                    {
                        Console.WriteLine("Failed to regenerate a valid token.");
                        return null;
                    }
                }

                return credentials;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating GoogleCredential: {e.Message}");
            }

            return null;
        }
        private static GoogleCredential? GenerateNewCredential(string credentialsJson)
        {
            try
            {
                //regenerates the credentials
                var newCredential = GoogleCredential.FromJson(credentialsJson).CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

                return newCredential;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error generating new credential: {e.Message}");
                return null;
            }
        }

        private static SheetsService OAuthAuthenticate(GoogleCredential credential)
        {
            return new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,

                //applicationName is not necessary for functionality but is recommended for debugging on googles side. When I implement multiple sheets, will become irrelevant
                ApplicationName = "API_testing"
            });
        }




        public static bool IsCredentialValid(GoogleCredential credentials, string spreadsheetID)
        {
            //this function checks to see if the the credentials used are valid and uses a lightweight test to ensure they work
            try
            {
                var service = OAuthAuthenticate(credentials);
                var request = service.Spreadsheets.Get(spreadsheetID);
                request.Execute();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Validation failed and generating a new token: {e.Message} ");
                return false;
            }
        }

        /* private static string APIURLCreator(string SheetID)
         {
             //Creates URL to call API with the adjustable SheetID
             const string urlstart = "https://sheets.googleapis.com/v4/spreadsheets/";
             const string urlend = "/values/sheet1";
             string url = urlstart + SheetID + urlend;
             return url;

         }
        */
    }
}
