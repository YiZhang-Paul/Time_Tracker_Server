using Core.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Service.Repositories;
using System.IO;

namespace Service.Test.Integration.Repositories
{
    [TestFixture]
    public class EventHistoryRepositoryTest
    {
        private EventHistoryRepository Subject { get; set; }

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Local.json", true, false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetSection("TimeTrackerDbConnectionString").Value;
            var options = new DbContextOptionsBuilder<TimeTrackerDbContext>().UseNpgsql(connectionString).Options;
            Subject = new EventHistoryRepository(new TimeTrackerDbContext(options));
        }
    }
}
