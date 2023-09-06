using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace Pattern.Synchro.Client
{
    public class ClientPushSynchro<T> : IClientPushSynchro 
        where T : IEntity, new()
    {
        private readonly SQLiteAsyncConnection db;

        public ClientPushSynchro(SQLiteAsyncConnection db)
        {
            this.db = db;
        }

        public async Task<List<IEntity>> GetEntities(DateTime lastUpdated)
        {
            return (await this.db.Table<T>()
                .Where(t => t.LastUpdated >= lastUpdated)
                .ToListAsync()).Cast<IEntity>().ToList();
        }
    }
}