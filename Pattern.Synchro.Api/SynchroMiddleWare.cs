using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pattern.Synchro.Api.Pull;
using Pattern.Synchro.Api.Push;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Api
{
    public class SynchroMiddleWare  : IMiddleware
    {
        private readonly IPullSynchro pullSynchro;
        private readonly IServerPushSynchro serverPushSynchro;
        private readonly IDeviceInformation deviceInformation;
        private readonly IDateTimeService dateTimeService;
        private readonly IServerCallback serverCallback;
        private readonly IEnumerable<TypeToSync> typesToSync;
        private IJsonTypeInfoResolver jsonTypeInfoResolver;

        public SynchroMiddleWare(
            IPullSynchro pullSynchro, 
            IServerPushSynchro serverPushSynchro, 
            IDeviceInformation deviceInformation,
            IDateTimeService dateTimeService,
            IServerCallback serverCallback,
            IEnumerable<TypeToSync> typesToSync)
        {
            this.pullSynchro = pullSynchro;
            this.serverPushSynchro = serverPushSynchro;
            this.deviceInformation = deviceInformation;
            this.dateTimeService = dateTimeService;
            this.serverCallback = serverCallback;
            this.typesToSync = typesToSync;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            this.jsonTypeInfoResolver = new EntityTypeResolver(typesToSync.Select(c=>c.Type).ToArray());
            if (context.Request.Path.Value.StartsWith("/synchro/end"))
            {
                var deviceId = Guid.Parse(context.Request.Query["deviceId"]);
                var entities = await Serializer.Deserialize<SynchroDevice>(context.Request.Body, this.jsonTypeInfoResolver);
                await this.deviceInformation.SaveLastSynchro(context, deviceId, entities.BeginServerDateTime, entities.LastLocalSyncDateTime, entities.Version).ConfigureAwait(false);
                return;
            }

            if (context.Request.Path.Value.StartsWith("/synchro/begin"))
            {
                await this.serverCallback.Begin(context.Request.Headers).ConfigureAwait(false);
                
                var deviceId = Guid.Parse(context.Request.Query["deviceId"]);
                var bytes = Encoding.UTF8.GetBytes(await Serializer.Serialize(new SynchroDevice
                {
                    BeginServerDateTime = this.dateTimeService.DateTimeNow(),
                    LastLocalSyncDateTime = (await this.deviceInformation.GetLastLocalSynchro(context, deviceId)) ?? DateTime.MinValue,
                }, this.jsonTypeInfoResolver));
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                return;
            }

            if (context.Request.Path.Value.StartsWith("/synchro"))
            {
                var version = context.Request.Query.ContainsKey("version") ? int.Parse(context.Request.Query["version"]) : 0;
                switch (context.Request.Method.ToUpperInvariant())
                {
                    case "GET":
                        var deviceId = Guid.Parse(context.Request.Query["deviceId"]);
                        var previousVersion = (await this.deviceInformation.GetVersion(context, deviceId)) ?? version;
                        var lastSynchro = await this.deviceInformation.GetLastSynchro(context, deviceId).ConfigureAwait(false) ?? DateTime.MinValue;

                        var cars = this.pullSynchro.GetPull(context, lastSynchro, previousVersion, version);

                        var bytes = Encoding.UTF8.GetBytes(await Serializer.Serialize(cars, this.jsonTypeInfoResolver));
                        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                        return;
                    case "POST":
                        var entities = await Serializer.Deserialize<List<IEntity>>(context.Request.Body, this.jsonTypeInfoResolver);
                        await this.serverPushSynchro.Push(context, entities, version).ConfigureAwait(false);
                        return;
                }
            }

            await next(context).ConfigureAwait(false);
        }
    }
}