using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pattern.Synchro.Api;
using Pattern.Synchro.Api.Push;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Sample.Api
{
    public class CarPushProvider : DbSetPushProvider<SampleDbContext, Car, Client.Car>
    {
        public CarPushProvider(SampleDbContext sampleDbContext, IDateTimeService dateTimeService) 
            : base(sampleDbContext, dateTimeService)
        {
        }

        public override async Task<bool> CanPush(IEntity entity, int version)
        {
            return await base.CanPush(entity, version) && version == 0;
        }

        protected override DbSet<Car> GetDbSet(SampleDbContext db)
        {
            return db.Cars;
        }

        protected override bool UpdateProperties(HttpContext context, Client.Car entity, Car car)
        {
            car.UserId = context.User.Identity.Name;
            if (car.Name != entity.Name)
            {
                car.Name = entity.Name;
                return true;
            }

            return false;
        }
    }
}