using Microsoft.EntityFrameworkCore;
using Pattern.Synchro.Api;

namespace Pattern.Synchro.Sample.Api
{
    public class SampleDbContext : DbContext, IDeviceDbContext<Device>
    {
        public SampleDbContext()
        {
        }

        public SampleDbContext(DbContextOptions<SampleDbContext> options)
            : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarV2> CarV2s { get; set; }
    }
}