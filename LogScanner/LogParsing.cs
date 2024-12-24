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
using static LogScanner.DataStorage;
using Microsoft.EntityFrameworkCore;



namespace LogScanner
{
    public class LogParsing
    {
        public int Id { get; set; }  // primary key
        public DateTime timestamp { get; set; }
        public string level { get; set; }
        public string message { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void StartUp()
        {
            try
            {
                // Ensure the database exists
               AppDbContext.InitializeDatabase();

                string foundPath = FindFolder("C://Users/", "LogData");
                if (!string.IsNullOrEmpty(foundPath))
                {
                    Console.WriteLine($"Mappen hittades: {foundPath}\n");

                    FileIdentification();

                    // Pass the found folder path to the AnomalyDetection class
                    AnomalyDetection anomalyDetection = new AnomalyDetection(foundPath);
                    anomalyDetection.Start();

                    Console.WriteLine("Press 'Enter' to stop monitoring...");
                    Console.ReadLine();

                    anomalyDetection.Stop();
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

        public List<LogParsing> LoadFromJson(string file)
        {
            try
            {
                // Deserialize JSON data
                string jsonString = File.ReadAllText(file);
                var flatList = JsonSerializer.Deserialize<List<LogParsing>>(jsonString);

                // Ensure data is non-null
                var logEntries = flatList ?? new List<LogParsing>();

                // Store the data into the database using AppDbContext
                using (var context = new AppDbContext())
                {
                    // Make sure Entries is properly defined as DbSet<LogParsing>
                    context.Entries.AddRange(logEntries); // Add all the log entries
                    context.SaveChanges(); // Save the data to the database
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Successfully loaded {logEntries.Count} entries from JSON file into the database.");
                Console.ResetColor();

                return logEntries; // Return the loaded entries (optional)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse JSON file: {file}. Error: {ex.Message}");
                return new List<LogParsing>();
            }
        }

        public List<LogParsing> CsvReading(string file)
        {
            try
            {
                using (var reader = new StreamReader(file))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(file);
                    Console.ResetColor();

                    // Read the records from the CSV file
                    var records = csv.GetRecords<LogParsing>().ToList();

                    // Store to the database
                    using (var context = new AppDbContext())
                    {
                        context.Entries.AddRange(records);
                        context.SaveChanges();
                    }

                    return records;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse .CSV file: {file} ERROR {ex.Message}");
                return new List<LogParsing>();
            }
        }

        public List<LogParsing> TxtReading(string file)
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
                    // Parse each line into a LogParsing object
                    // This assumes a specific format
                    if (line.Contains("level"))
                    {
                        var parts = line.Split(',');
                        var log = new LogParsing
                        {
                            level = parts[0].Split(':')[1].Trim(),
                            timestamp = DateTime.Parse(parts[1].Split(':')[1].Trim()),
                            message = parts[2].Split(':')[1].Trim()
                        };
                        result.Add(log);
                    }
                }

                // Store the logs into the database
                using (var context = new AppDbContext(new DbContextOptions<AppDbContext>()))
                {
                    context.Entries.AddRange(result);
                    context.SaveChanges();
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse .TXT file: {file} ERROR {ex.Message}");
                return new List<LogParsing>();
            }
        }

        public void FileIdentification()
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
                        CsvReading(file);  // Call CSV reading method
                        break;
                    case ".json":
                        Console.WriteLine($"File: {file} - Detected as JSON");
                        LoadFromJson(file);  // Call JSON reading method
                        break;
                    case ".txt":
                        Console.WriteLine($"File: {file} - Detected as TXT");
                        TxtReading(file);  // Call TXT reading method
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

        private readonly Dictionary<string, long> _fileLineTracker = new Dictionary<string, long>();


        public void OverWatch(string filePath)
        {
            const int maxRetries = 5; // Maximum number of retries
            const int delayBetweenRetries = 500; // Delay in milliseconds between retries

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    long lastLineProcessed = _fileLineTracker.ContainsKey(filePath) ? _fileLineTracker[filePath] : 0;

                    // Open the file with shared read access
                    using (var reader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        var currentLine = 0;
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            currentLine++;
                            if (currentLine <= lastLineProcessed)
                            {
                                // Skip lines already processed
                                continue;
                            }

                            // Parse the line into a LogParsing object
                            LogParsing log = ParseLogLine(line);

                            // Check for anomalies
                            if (log != null && (log.level == "WARN" || log.level == "ERROR"))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Anomaly Detected! Level: {log.level}, Message: {log.message}, Timestamp: {log.timestamp}");
                                Console.ResetColor();
                            }
                        }
                        // Update the last processed line
                        _fileLineTracker[filePath] = currentLine;
                    }
                    // If we successfully processed the file, exit the retry loop
                    return;
                }
                catch (IOException)
                {
                    if (attempt < maxRetries - 1)
                    {
                        // Wait before retrying
                        System.Threading.Thread.Sleep(delayBetweenRetries);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to process file: {filePath}. File is locked or inaccessible.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred while processing file: {filePath}. Error: {ex.Message}");
                    return;
                }
            }
        }

        // Helper method to parse a log line
        private LogParsing ParseLogLine(string line)
        {
            // Example parser for a line in the format: "level: WARN, timestamp: 2024-12-18, message: Some warning message"
            try
            {
                if (line.Contains("level"))
                {
                    var parts = line.Split(',');
                    return new LogParsing
                    {
                        level = parts[0].Split(':')[1].Trim(),
                        timestamp = DateTime.Parse(parts[1].Split(':')[1].Trim()),
                        message = parts[2].Split(':')[1].Trim()
                    };
                }
            }
            catch
            {
                Console.WriteLine($"Failed to parse line: {line}");
            }
            return null;
        }
    }
}

































