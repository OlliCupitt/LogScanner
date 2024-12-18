

using CsvHelper;
using System.Security.Cryptography.X509Certificates;

namespace LogScanner
{
    internal class Program
    {
       
        static void Main(string[] args)
        {
            var logparsing = new LogParsing();



            logparsing.StartUp();


           
        }
        


    }
}
        //    //string filesDirectory = "C://Users/ollic/source/repos/LogScanner/LogScanner/bin/Debug/net9.0";

        //    string[] fileNames = new string[] { "example_logs_1", "example_logs_2", "example_logs_3", "example_logs_4", "example_logs" };  // Define the file names to search for
        //                                                                                                                                   // can change later with getdirectory
        //    LogParsing.SearchAndProcessFiles(filesDirectory, fileNames);  //test 
//        //}
//    }
//}
