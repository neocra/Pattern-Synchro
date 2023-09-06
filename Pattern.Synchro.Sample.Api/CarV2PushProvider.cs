using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pattern.Synchro.Api;
using Pattern.Synchro.Api.Push;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Sample.Api
{
    public class CarV2PushProvider : DbSetPushProvider<SampleDbContext, CarV2, Client.Car>
    {
        public CarV2PushProvider(SampleDbContext sampleDbContext, IDateTimeService dateTimeService) 
            : base(sampleDbContext, dateTimeService)
        {
        }

        public override async Task<bool> CanPush(IEntity entity, int version)
        {
            return await base.CanPush(entity, version) && version == 2;
        }

        protected override DbSet<CarV2> GetDbSet(SampleDbContext db)
        {
            return db.CarV2s;
        }

        protected override bool UpdateProperties(HttpContext context, Client.Car entity, CarV2 car)
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