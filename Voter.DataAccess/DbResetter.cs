using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Voter.DataAccess
{
    public static class DbResetter
    {
        public static async Task ResetDatabase(IServiceProvider serviceProvider)
        {
            try
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                VoterDbContext dbContext = scope.ServiceProvider.GetRequiredService<VoterDbContext>();

                // Close all existing connections to the database
                dbContext.Database.CloseConnection();

                // Give time for connections to close
                await Task.Delay(1000);

                // Delete the database
                _ = await dbContext.Database.EnsureDeletedAsync();

                // For SQLite specifically, also try to delete the file directly if it exists
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "voter.db");
                if (File.Exists(dbPath))
                {
                    try
                    {
                        File.Delete(dbPath);
                        Console.WriteLine("Database file deleted manually.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not manually delete database file: {ex.Message}");
                    }
                }

                // Create a new database
                await dbContext.Database.MigrateAsync();

                // Seed the database with initial data
                await DbInitializer.SeedDatabase(serviceProvider);

                Console.WriteLine("Database reset completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting database: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                // Rethrow the exception so the caller knows about the failure
                throw;
            }
        }
    }
}
