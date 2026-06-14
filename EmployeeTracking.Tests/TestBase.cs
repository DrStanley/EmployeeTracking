using EmployeeTracking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected readonly AppDbContext Context;

        protected TestBase()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Context = new AppDbContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
