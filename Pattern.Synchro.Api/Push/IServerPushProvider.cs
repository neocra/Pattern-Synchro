using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Api.Push
{
    public interface IServerPushProvider
    {
        Task<bool> CanPush(IEntity entity, int version);

        Task Push(HttpContext context, IEntity entity);
    }
}