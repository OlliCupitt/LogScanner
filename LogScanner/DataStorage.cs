using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace LogScanner
{
    public class DataStorage
    {
        // The DbContext class for the database
        public class AppDbContext : DbContext
        {
            // This DbSet will hold the log entries
            public DbSet<LogParsing> Entries { get; set; }

            // Configure the DbContext with a connection string to the SQL Server database
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=EntryDatabase;Trusted_Connection=True;");
            }

            // Method to initialize the database (ensure it's created if it doesn't exist)
            public static void InitializeDatabase()
            {
                using (var context = new AppDbContext())
                {
                    // Ensure the database is created if it doesn't exist
                    context.Database.EnsureCreated();
                }
            }
        }
    }


}
