using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Api.Push
{
    public interface IServerPushSynchro
    {
        Task Push(HttpContext context, List<IEntity> entities, int version);
    }
}