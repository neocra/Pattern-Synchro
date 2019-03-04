using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Api.Push
{
    public abstract class ServerPushProviderBase<T> : IServerPushProvider
    {
        public Task<bool> CanPush(IEntity entity)
        {
            return Task.FromResult(entity is T);
        }

        public Task Push(HttpContext context, IEntity entity)
        {
            return this.Push(context, (T) entity);
        }

        protected abstract Task Push(HttpContext context, T entity);
    }
}