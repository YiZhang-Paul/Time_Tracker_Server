using Core.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Service.Test
{
    public class DatabaseTestUtility
    {
        public async Task<TimeTrackerDbContext> SetupTimeTrackerDbContext()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Local.json", true, false)
                .AddEnvironmentVariables()
                .Build();

            var connection = config.GetSection("TimeTrackerTestDbConnectionString").Value;
            var builder = new DbContextOptionsBuilder<TimeTrackerDbContext>().UseNpgsql(connection);
            var context = new TimeTrackerDbContext(builder.Options);

            return await context.Database.EnsureCreatedAsync().ConfigureAwait(false) ? context : null;
        }
    }
}
