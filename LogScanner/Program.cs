using System.Diagnostics;

namespace LogScanner
{
    public class Program
    {
        static void Main(string[] args)
        {
            string filesDirectory = "C://Users/ollic/source/repos/LogScanner/LogScanner/bin/Debug/net9.0";

            // Define the file names to search for
            string[] fileNames = new string[] { "example_logs_1", "example_logs_2", "example_logs_3", "example_logs_4", "example_logs" };

            // Call the method to search and process files
             LogParsing.SearchAndProcessFiles(filesDirectory, fileNames);
        }
    }
}
