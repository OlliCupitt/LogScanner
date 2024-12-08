using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;


namespace LogScanner
{
    public class LogParsing
    {
        public static void SearchAndProcessFiles(string directoryPath, string[] fileNames)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("The specified directory does not exist.");
                return;
            }

            string[] files = Directory.GetFiles(directoryPath);  // Get all files in the directory

            foreach (string file in files)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);    // Extract the file name without extension

                if (Array.Exists(fileNames, name => name.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase)))    // Check if the file name matches one of the predefined names
                {

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
        }
        public static void LoadFromJson(string file)                                          // Loads parking spots from a JSON file
        {

            string jsonString = File.ReadAllText(file);
            //var flatList = JsonSerializer.Deserialize<List<LogParsing>>(jsonString);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(jsonString);
            Console.ResetColor();
        }
            
        public static void CsvReading(string file)
        {
            string csvString = File.ReadAllText(file);
            //var lines = csvString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            Console.ForegroundColor= ConsoleColor.Red;
            Console.WriteLine(csvString);
            Console.ResetColor();
        }

        public static void TxtReading(string file)
        {
            string txtString = File.ReadAllText(file);
            //var lines = txtString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            Console.ForegroundColor= ConsoleColor.Yellow;
            Console.WriteLine(txtString);
            Console.ResetColor();
        }
    }

}
































