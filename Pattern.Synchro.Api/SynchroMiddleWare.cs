using System;
using System.Collections.Generic;
using System.Text;
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

        public SynchroMiddleWare(
            IPullSynchro pullSynchro, 
            IServerPushSynchro serverPushSynchro, 
            IDeviceInformation deviceInformation,
            IDateTimeService dateTimeService,
            IServerCallback serverCallback)
        {
            this.pullSynchro = pullSynchro;
            this.serverPushSynchro = serverPushSynchro;
            this.deviceInformation = deviceInformation;
            this.dateTimeService = dateTimeService;
            this.serverCallback = serverCallback;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Path.Value.StartsWith("/synchro/end"))
            {
                var deviceId = Guid.Parse(context.Request.Query["deviceId"]);
                var entities = await Serializer.Deserialize<SynchroDevice>(context.Request.Body);
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
                }));
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

                        var bytes = Encoding.UTF8.GetBytes(await Serializer.Serialize(cars));
                        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                        return;
                    case "POST":
                        var entities = await Serializer.Deserialize<List<IEntity>>(context.Request.Body);
                        await this.serverPushSynchro.Push(context, entities, version).ConfigureAwait(false);
                        return;
                }
            }

            await next(context).ConfigureAwait(false);
        }
    }
}