using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogScanner
{
    public class AnomalyDetection
    {
        


        private FileSystemWatcher _watcher;

        private readonly HashSet<string> _processedFiles = new HashSet<string>(); // For debouncing events
        string filePath = LogParsing.FindFolder("C://Users/", "LogData");
        public AnomalyDetection(string filepath, string fileFilter = "*.*", bool includeSubdirectories = false)
        {
            if (!Directory.Exists(filePath))
            {
                throw new DirectoryNotFoundException($"The directory '{filePath}' does not exist.");
            }

            // Initialize the FileSystemWatcher
            _watcher = new FileSystemWatcher
            {
                Path = filePath,
                Filter = fileFilter,
                IncludeSubdirectories = includeSubdirectories,
                EnableRaisingEvents = false // Start disabled; enable explicitly later
            };

            // Attach event handlers
            _watcher.Created += OnCreated;
            _watcher.Changed += OnChanged;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;
        }

        // Start monitoring
        public void Start()
        {
            Console.WriteLine("Starting anomaly detection...");
            _watcher.EnableRaisingEvents = true;
        }

        // Stop monitoring
        public void Stop()
        {
            Console.WriteLine("Stopping anomaly detection...");
            _watcher.EnableRaisingEvents = false;
        }

        // Dispose of resources
        public void Dispose()
        {
            _watcher?.Dispose();
        }

        // Event handlers
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"[CREATED] File detected: {e.FullPath}");
            HandleAnomaly(e.FullPath, "created");

            LogParsing logParser = new LogParsing();
            logParser.OverWatch(e.FullPath);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (_processedFiles.Add(e.FullPath)) // Debounce multiple events for the same file
            {
                Console.WriteLine($"[CHANGED] File modified: {e.FullPath}");
                HandleAnomaly(e.FullPath, "changed");
                
                LogParsing logParsing = new LogParsing();
                logParsing.OverWatch(e.FullPath);
                // Clear after a delay
                System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ => _processedFiles.Remove(e.FullPath));
            }
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"[DELETED] File removed: {e.FullPath}");
            HandleAnomaly(e.FullPath, "deleted");
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"[RENAMED] File renamed: {e.OldFullPath} -> {e.FullPath}");
            HandleAnomaly(e.FullPath, "renamed", e.OldFullPath);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"[ERROR] An error occurred: {e.GetException().Message}");
        }

        // Custom method to handle anomalies
        private void HandleAnomaly(string filePath, string changeType, string oldFilePath = null)
        {
            // Add custom anomaly detection logic here
            Console.WriteLine($"Anomaly detected: {changeType} -> {filePath}");
            if (!string.IsNullOrEmpty(oldFilePath))
            {
                Console.WriteLine($"Old path: {oldFilePath}");
            }
        }

       
    }
}

