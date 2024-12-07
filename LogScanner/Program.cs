namespace LogScanner
{
    public class Program
    {
        static void Main(string[] args)
        {
            LogParsing logParser = new LogParsing();

            // Example file names (no extensions)
            string[] fileNames = new string[] { "Log_1", "Log_2", "log_3", "Log_4", "Log_5" };

            string filesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "net9.0");
            Console.WriteLine("Files Directory: " + filesDirectory);

            // Use the FindFilesAndIdentifyType method to find and identify the files
            foreach (var fileTypeResult in logParser.FindFilesAndIdentifyType(filesDirectory, fileNames))
            {
                if (fileTypeResult.FilePath != null)
                {
                    Console.WriteLine($"Found file: {fileTypeResult.FileName} as a {fileTypeResult.FileType} file.");
                }
                else
                {
                    Console.WriteLine($"File {fileTypeResult.FileName} not found.");
                }
            }
        }
    }
}
