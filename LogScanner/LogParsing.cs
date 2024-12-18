using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using CsvHelper;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;


namespace LogScanner
{
    public class LogParsing
    {
        public string message { get; set; }

        public string level { get; set; }
        public DateTime timestamp { get; set; }



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void StartUp()
        {
            try
            {
                string foundPath = FindFolder("C://Users/", "LogData");

                if (!string.IsNullOrEmpty(foundPath))
                {
                    Console.WriteLine($"Mappen hittades: {foundPath}\n");
                }
                else
                {
                    Console.WriteLine($"Mappen hittades inte.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel uppstod: {ex.Message}");
            }

            HorUnge();
        }



        public void LoadFromJson(string file)
        {
            string jsonString = File.ReadAllText(file);
            var flatList = JsonSerializer.Deserialize<List<LogParsing>>(jsonString);           // Deserialize the JSON into a list of LogParsing objects
            string jsonOutput = JsonSerializer.Serialize(flatList, new JsonSerializerOptions { WriteIndented = true });       // Serialize the object back to JSON for pretty printing

            Console.ForegroundColor = ConsoleColor.Green;          // Print the serialized JSON to the console
            Console.WriteLine(jsonOutput);
            Console.ResetColor();
        }

        public void CsvReading(string file)
        {
            string csvData = File.ReadAllText(file);
            //var lines = csvString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            using (var reader = new StringReader(csvData))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(csvData);
            Console.ResetColor();
        }

        public void TxtReading(string file)
        {
            string txtString = File.ReadAllText(file);
            //var lines = txtString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(txtString);
            Console.ResetColor();

        }

        public void HorUnge()
        {
            string[] files = Directory.GetFiles(FindFolder("C://Users/", "LogData"));  // Get all files in the directory

            foreach (string file in files)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);    // Extract the file name without extension


                string fileExtension = Path.GetExtension(file).ToLower();     // Determine the file type based on the extension
                switch (fileExtension)
                {
                    case ".csv":
                        Console.WriteLine($"File: {file} - Detected as CSV");
                        CsvReading(file);                                      // Call a method to handle CSV files
                        break;
                    case ".json":
                        Console.WriteLine($"File: {file} - Detected as JSON");
                        LoadFromJson(file);                                     // Call a method to handle JSON files
                        break;
                    case ".txt":
                        Console.WriteLine($"File: {file} - Detected as TXT");
                        TxtReading(file);                                       // Call a method to handle TXT files
                        break;
                    default:
                        Console.WriteLine($"File: {file} - Unknown or unsupported file type");
                        break;
                }

            }
        }
        public static string FindFolder(string startDirectory, string mappNamn)
        {
            try
            {
                // Search in the current directory for a matching folder name
                foreach (var folder in Directory.EnumerateDirectories(startDirectory))
                {
                    if (Path.GetFileName(folder).Equals(mappNamn, StringComparison.OrdinalIgnoreCase))
                    {
                        return folder; // Found the folder
                    }

                    // Recursively search in subdirectories by passing the current subfolder
                    string found = FindFolder(folder, mappNamn);
                    if (!string.IsNullOrEmpty(found))
                    {
                        return found; // Return if found in subdirectories
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories where access is denied
            }
            catch (DirectoryNotFoundException)
            {
                // Skip directories that no longer exist
            }

            return null; // Folder not found
        }
    }
}

































