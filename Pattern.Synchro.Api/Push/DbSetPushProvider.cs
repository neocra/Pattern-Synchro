using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Api.Push
{
    public abstract class DbSetPushProvider<T, TModel, TDto> : ServerPushProviderBase<TDto> 
        where T : DbContext
        where TModel : class, IEntity, new()
        where TDto : IEntity
    {
        private readonly T db;
        private readonly IDateTimeService dateTimeService;

        protected DbSetPushProvider(T db, IDateTimeService dateTimeService)
        {
            this.db = db;
            this.dateTimeService = dateTimeService;
        }

        protected abstract DbSet<TModel> GetDbSet(T db);
        
        protected override async Task Push(HttpContext context, TDto entity)
        {
            var car = await this.GetDbSet(this.db).FindAsync(entity.Id);

            if (car == null)
            {
                car = new TModel
                {
                    Id = entity.Id,
                    LastUpdated = this.dateTimeService.DateTimeNow()
                };

                car.IsDeleted = entity.IsDeleted;
                this.UpdateProperties(context, entity, car);
                
                await this.GetDbSet(this.db).AddAsync(car);                
            }
            else
            {
                if (this.UpdateProperties(context, entity, car)
                    || car.IsDeleted != entity.IsDeleted)
                {
                    car.IsDeleted = entity.IsDeleted;
                    car.LastUpdated = this.dateTimeService.DateTimeNow();
                }
            }

            await this.db.SaveChangesAsync();
        }

        protected abstract bool UpdateProperties(HttpContext context, TDto entity, TModel car);
    }
}