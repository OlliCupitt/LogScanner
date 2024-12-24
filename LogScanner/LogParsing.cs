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
using CsvHelper.Configuration;
using System.ComponentModel.DataAnnotations;

namespace LogScanner
{
    public class LogParsing
    {
        [Key]
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
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                };

                using (var reader = new StreamReader(file))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(file);
                    Console.ResetColor();

                    var records = csv.GetRecords<LogParsing>().ToList();

                    using (var context = new AppDbContext())
                    {
                        foreach (var record in records)
                        {
                            // Use EF Core's Upsert functionality (if available)
                            context.Entries.Update(record);
                        }
                        context.SaveChanges();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Successfully loaded {records.Count} entries from CSV file into the database.");
                        Console.ResetColor();
                    }

                    return records;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse .CSV file: {file}. Error: {ex.Message}");
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
                    // Ensure the line has the correct format before attempting to parse it
                    if (line.Contains("]"))
                    {
                        try
                        {
                            var timestampEndIndex = line.IndexOf("]") + 1;
                            var timestampString = line.Substring(1, timestampEndIndex - 2);
                            var levelAndMessage = line.Substring(timestampEndIndex).Trim();

                            var levelEndIndex = levelAndMessage.IndexOf(":");
                            if (levelEndIndex > 0)
                            {
                                var level = levelAndMessage.Substring(0, levelEndIndex).Trim();
                                var message = levelAndMessage.Substring(levelEndIndex + 1).Trim();

                                var log = new LogParsing
                                {
                                    level = level,
                                    timestamp = DateTime.Parse(timestampString),
                                    message = message
                                };

                                result.Add(log);
                            }
                            else
                            {
                                Console.WriteLine($"Skipping malformed line: {line}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing line: {line}. Error: {ex.Message}");
                        }
                    }
                }

                // Debugging: Output the result list size and content before saving
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Successfully processed {result.Count} entries from TXT file.");
                Console.ResetColor();

                if (result.Count == 0)
                {
                    Console.WriteLine("No valid entries were parsed from the TXT file.");
                }

                // Store the logs into the database
                if (result.Count > 0)
                {
                    using (var context = new AppDbContext())
                    {
                        try
                        {
                            Console.WriteLine($"Attempting to add {result.Count} entries to the database...");
                            context.Entries.AddRange(result);

                            // Debugging: Output the entity count before saving
                            Console.WriteLine("Entries to be saved: ");
                            foreach (var log in result)
                            {
                                Console.WriteLine($"{log.timestamp} | {log.level} | {log.message}");
                            }

                            context.SaveChanges();
                            Console.WriteLine("Successfully saved entries to the database.");
                        }
                        catch (Exception dbEx)
                        {
                            Console.WriteLine($"Failed to save entries to the database. Error: {dbEx.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No valid entries were found to be saved.");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse .TXT file: {file}. Error: {ex.Message}");
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

                    using (var reader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))       // Open the file with shared read access
                    {
                        var currentLine = 0;
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            currentLine++;
                            if (currentLine <= lastLineProcessed)
                            {
                                continue;       // Skip lines already processed
                            }

                            LogParsing log = ParseLogLine(line);        // Parse the line into a LogParsing object

                            if (log != null && (log.level == "WARN" || log.level == "ERROR"))       // Check for anomalies
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Anomaly Detected! Level: {log.level}, Message: {log.message}, Timestamp: {log.timestamp}");
                                Console.ResetColor();
                            }


                            using (var context = new AppDbContext())    // Check if the log exists in the database (based on timestamp)
                            {
                                var existingLog = context.Entries
                                    .FirstOrDefault(l => l.timestamp == log.timestamp);

                                if (existingLog != null)   // Update the existing log entry
                                {
                                    existingLog.level = log.level;
                                    existingLog.message = log.message;
                                }
                                else     // Add a new log entry
                                {
                                    context.Entries.Add(log);
                                }
                                context.SaveChanges();    // Save changes to the database
                            }
                        }
                        _fileLineTracker[filePath] = currentLine;    // Update the last processed line
                    }

                    return;    // If we successfully processed the file, exit the retry loop
                }
                catch (IOException)
                {
                    if (attempt < maxRetries - 1)   // Wait before retrying
                    {
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



