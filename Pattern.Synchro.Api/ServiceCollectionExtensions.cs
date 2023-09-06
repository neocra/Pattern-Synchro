using Microsoft.Extensions.DependencyInjection;
using Pattern.Synchro.Api.Pull;
using Pattern.Synchro.Api.Push;

namespace Pattern.Synchro.Api
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSynchro(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDateTimeService, DateTimeService>();
            serviceCollection.AddTransient<SynchroMiddleWare>();
            serviceCollection.AddTransient<IServerPushSynchro, PushSynchro>();
            serviceCollection.AddTransient<IPullSynchro, PullSynchro>();
        }
    }
}