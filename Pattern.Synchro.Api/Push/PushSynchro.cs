using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Api.Push
{
    public class PushSynchro : IServerPushSynchro
    {
        private readonly IEnumerable<IServerPushProvider> pushProviders;

        public PushSynchro(IEnumerable<IServerPushProvider> pushProviders)
        {
            this.pushProviders = pushProviders;
        }
        
        public async Task Push(HttpContext context, List<IEntity> entities, int version)
        {
            foreach (var pushProvider in this.pushProviders)
            {
                foreach (var entity in entities)
                {
                    if (await pushProvider.CanPush(entity, version))
                    {
                        await pushProvider.Push(context, entity);
                    }   
                }
            }
        }
    }
}