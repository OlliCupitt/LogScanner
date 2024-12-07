using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace LogScanner
{
    public class LogParsing
    {
        string[] fileNames = new string[] { "example_logs_1", "example_logs_2", "example_logs_3", "example_logs_4", "example_logs" };
        // string jsonFileName_1 = "example_logs_1";
        //string jsonFileName_2 = "example_logs_2";
        //string csvFileName_1 = "example_logs_2";
        //string csvFileName_2 = "example_logs_2";
        //string txtFileName_1 = "example_logs";
        
        string filesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Debug", "net9.0");           // Navigate to the 'bin\Debug\net9.0' folder (it should already be there)


        public (string FilePath, string FileName, string FileType)[] FindFilesAndIdentifyType(string startDirectory, params string[] fileNames)
        {

            string[] possibleExtensions = { ".txt", ".csv", ".json" };  // fole type addons
            var results = new (string, string, string)[fileNames.Length]; // Store results for each file

            for (int i = 0; i < fileNames.Length; i++)
            {
                string fileName = fileNames[i];
                string filePath = null;
                string fileType = "Unknown";

                foreach (string extension in possibleExtensions)
                {
                    filePath = Path.Combine(startDirectory, fileName + extension);
                    if (File.Exists(filePath))
                    {
                        // Identify the file type based on its content
                        fileType = IdentifyFileType(filePath);
                        break; // Stop searching once the file is found
                    }
                }
                // Store the result for this file
                results[i] = (filePath, fileName, fileType);
            }

            return results;
        }

        private string IdentifyFileType(string filePath)
        {
            string content = File.ReadAllText(filePath);

            // Try to identify the content type (JSON, CSV, or plain text)
            if (content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("["))
            {
                try
                {
                    // Attempt to parse as JSON
                    var obj = System.Text.Json.JsonDocument.Parse(content);
                    return "JSON";
                }
                catch
                {
                    // Not JSON, move on
                }
            }

            if (content.Contains(",") && content.Split('\n')[0].Split(',').Length > 1)
            {
                return "CSV";
            }

            return "Plain Text"; // Default to plain text if nothing else matches
        }
    }

























}


        
