using System.Linq;
using Microsoft.EntityFrameworkCore;
using Pattern.Synchro.Sample.Api;

namespace Pattern.Synchro.Tests;

public class DbContextClassFixture
{
    public SampleDbContext DbContext { get; private set; }

    public void InitDbContext(string serverDatabaseName)
    {
        if (this.DbContext == null)
        {
            var options = new DbContextOptionsBuilder<SampleDbContext>()
                .UseSqlite($"Data Source={serverDatabaseName}")
                .Options;

            this.DbContext = new SampleDbContext(options);
            this.DbContext.Database.EnsureCreated();
        }

        this.Clean(this.DbContext.Cars);
        this.Clean(this.DbContext.CarV2s);
        this.Clean(this.DbContext.Devices);
    }

    private void Clean<T>(DbSet<T> table) where T : class
    {
        var cars = table.ToList();
        this.DbContext.RemoveRange(cars);
        this.DbContext.SaveChanges();
    }
}