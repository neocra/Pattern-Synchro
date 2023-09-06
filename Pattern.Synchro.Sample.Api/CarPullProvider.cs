using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pattern.Synchro.Api.Pull;

namespace Pattern.Synchro.Sample.Api
{
    public class CarPullProvider : DbSetPullProvider<SampleDbContext, Car, Client.Car>
    {
        public CarPullProvider(SampleDbContext sampleDbContext) 
            : base(sampleDbContext)
        {
        }

        protected override DbSet<Car> GetDbSet(SampleDbContext db)
        {
            return db.Cars;
        }

        protected override void UpdateProperties(Client.Car entity, Car car)
        {
            entity.Name = car.Name;
        }

        protected override IQueryable<Car> AddFilter(DbSet<Car> dbSet, HttpContext context, DateTime lastSynchro)
        {
            return dbSet.Where(c => c.UserId == context.User.Identity.Name);
        }
    }
}