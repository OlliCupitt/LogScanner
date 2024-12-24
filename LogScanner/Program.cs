

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
       