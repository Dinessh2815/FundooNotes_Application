using DataBaseLayer.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace FundooNotes.Tests
{
    [TestFixture]
    public abstract class TestBase
    {
        protected FundooNotesDbContext _context = null!;
        protected IConfiguration _configuration = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<FundooNotesDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FundooNotesDbContext(options);

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
                .Build();

            OnSetUp();
        }

        [TearDown]
        public void TearDown()
        {
            OnTearDown();
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }

        protected virtual void OnSetUp() { }
        protected virtual void OnTearDown() { }

        protected async Task SeedTestDataAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
