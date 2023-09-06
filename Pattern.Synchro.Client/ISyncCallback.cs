using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pattern.Synchro.Client
{
    public interface ISyncCallback
    {
        Task SyncEvents(SyncEvent @event, List<IEntity> entities);
    }
}