using System;
using Microsoft.Extensions.DependencyInjection;
using Pattern.Synchro.Api.Pull;
using Pattern.Synchro.Api.Push;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Api
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSynchro(this IServiceCollection serviceCollection, Type[] types)
        {
            serviceCollection.AddTransient<IDateTimeService, DateTimeService>();
            serviceCollection.AddTransient<SynchroMiddleWare>();
            serviceCollection.AddTransient<IServerPushSynchro, PushSynchro>();
            serviceCollection.AddTransient<IPullSynchro, PullSynchro>();

            foreach (var typeToSync in types)
            {
                serviceCollection.AddTransient(c =>new TypeToSync(typeToSync));
            }
        }
    }
}