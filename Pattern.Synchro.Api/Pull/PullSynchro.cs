using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Api.Pull
{
    public class PullSynchro : IPullSynchro
    {
        private readonly IEnumerable<IServerPullProvider> serverPullProviders;

        public PullSynchro(IEnumerable<IServerPullProvider> serverPullProviders)
        {
            this.serverPullProviders = serverPullProviders;
        }

        public List<IEntity> GetPull(HttpContext context, DateTime lastSynchro, int previousVersion, int version)
        {
            var entities = new List<IEntity>();
            foreach (var serverPullProvider in this.serverPullProviders)
            {
                entities.AddRange(serverPullProvider.GetPull(context, lastSynchro, previousVersion, version));
            }

            return entities;
        }
    }
}