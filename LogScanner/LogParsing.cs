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


        public static void SearchAndProcessFiles(string directoryPath, string[] fileNames)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("The specified directory does not exist.");
                return;
            }

            // Get all files in the directory
            string[] files = Directory.GetFiles(directoryPath);

            foreach (string file in files)
            {
                // Extract the file name without extension
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                // Check if the file name matches one of the predefined names
                if (Array.Exists(fileNames, name => name.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase)))
                {
                    // Determine the file type based on the extension
                    string fileExtension = Path.GetExtension(file).ToLower();
                    switch (fileExtension)
                    {
                        case ".csv":
                            Console.WriteLine($"File: {file} - Detected as CSV");
                            // Call a method to handle CSV files
                            break;
                        case ".json":
                            Console.WriteLine($"File: {file} - Detected as JSON");
                            // Call a method to handle JSON files
                            break;
                        case ".txt":
                            Console.WriteLine($"File: {file} - Detected as TXT");
                            // Call a method to handle TXT files
                            break;
                        default:
                            Console.WriteLine($"File: {file} - Unknown or unsupported file type");
                            break;
                    }
                }
            }
        }
    }

























}



