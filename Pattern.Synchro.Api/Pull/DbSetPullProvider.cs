using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Api.Pull
{
    public abstract class DbSetPullProvider<T, TModel, TDto> : IServerPullProvider
        where T : DbContext
        where TModel : class, IEntity, new()
        where TDto : IEntity, new()
    {
        private readonly T db;

        protected DbSetPullProvider(T db)
        {
            this.db = db;
        }

        protected abstract DbSet<TModel> GetDbSet(T db);
        
        protected abstract void UpdateProperties(TDto entity, TModel car);
        public List<IEntity> GetPull(HttpContext context, DateTime lastSynchro)
        {
            return AddFilter(this.GetDbSet(this.db), context, lastSynchro)
                .Where(c => c.LastUpdated >= lastSynchro)
                .ToList()
                .Select(c =>
                {
                    var dto = new TDto
                    {
                        Id = c.Id,
                    };
                    this.UpdateProperties(dto, c);
                    return dto;
                }).Cast<IEntity>().ToList();
        }

        protected virtual IQueryable<TModel> AddFilter(DbSet<TModel> dbSet, HttpContext context, DateTime lastSynchro)
        {
            return dbSet;
        }
    }
}