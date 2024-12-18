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

                    HorUnge();

                    // Pass the found folder path to the AnomalyDetection class
                    AnomalyDetection anomalyDetection = new AnomalyDetection(foundPath);
                    anomalyDetection.Start();  // Start monitoring the folder


                    Console.WriteLine("Press 'Enter' to stop monitoring...");
                    Console.ReadLine(); // Wait for user input to stop the monitoring

                    anomalyDetection.Stop();  // Stop monitoring when the user presses Enter
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

           
        }

       

        public List<LogParsing>LoadFromJson(string file)
        {
            try
            {
                string jsonString = File.ReadAllText(file);

                var flatList = JsonSerializer.Deserialize<List<LogParsing>>(jsonString);
                Console.ForegroundColor = ConsoleColor.Green;                               // Deserialize the JSON into a list of LogParsing objects
                Console.WriteLine(file);                                 
                Console.ResetColor();
                return flatList ?? new List<LogParsing>();
            }    
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse .JSON file: {file} ERROR {ex.Message}");
                return new List<LogParsing>();
            }
                                                   // Print the serialized JSON to the console
        }

        public List<LogParsing>CsvReading(string file)
        {
            try
            {
                using (var reader = new StreamReader(file))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(file);
                    Console.ResetColor(); 
                    return csv.GetRecords<LogParsing>().ToList(); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse .CSV file: {file} ERROR {ex.Message}");
                return new List<LogParsing>();
            }
        }

        public List<LogParsing>TxtReading(string file)
        {
            try
            {
                string[] lines = File.ReadAllLines(file);
                var result = new List<LogParsing>();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(file);
                Console.ResetColor();
                foreach (var line in lines)
                {
                    // Parse each line into a LogParsing object (requires specific format assumptions)
                    // Example:
                    // level: WARN, timestamp: 2024-12-18, message: Some warning message

                    if (line.Contains("level"))
                    {
                        var parts = line.Split(',');
                        result.Add(new LogParsing
                        {
                            level = parts[0].Split(':')[1].Trim(),
                            timestamp = DateTime.Parse(parts[1].Split(':')[1].Trim()),
                            message = parts[2].Split(':')[1].Trim()
                        });
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse .TXT file: {file} ERROR {ex.Message}");
                return new List<LogParsing>();
            }
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


        public void OverWatch(string filepath)
        {
           List<LogParsing> logs = new List<LogParsing>();
            string fileExtension = Path.GetExtension(filepath).ToLower();

            switch (fileExtension)
            {
                case ".json":
                    logs = LoadFromJson(filepath); break;
                case ".csv":
                    logs = CsvReading(filepath); break;
                case ".txt":
                    logs = TxtReading(filepath); break;
                default:
                    Console.WriteLine($"Unsupported file type: {filepath}");
                    return;
            }
            foreach (var log in logs)
            {
                if (log.level == "WARN" ||  log.level == "ERROR")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Anomaly Detected! Level: {log.level}, Message: {log.message}, Timestamp: {log.timestamp}");
                    Console.ResetColor();
                }
            }


        }
    }
}

































