using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pattern.Synchro.Client
{
    public interface IClientPushSynchro
    {
        Task<List<IEntity>> GetEntities(DateTime lastUpdated);
    }
}