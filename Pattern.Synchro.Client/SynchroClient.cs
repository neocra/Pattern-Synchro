using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using SQLite;

namespace Pattern.Synchro.Client
{
    public class SynchroClient
    {
        private readonly HttpClient httpClient;
        private readonly SQLiteAsyncConnection db;
        private readonly IEnumerable<IClientPushSynchro> clientPushSynchro;
        private ISyncCallback syncCallback;
        private readonly IJsonTypeInfoResolver jsonTypeInfoResolver;

        public SynchroClient(HttpClient httpClient, SQLiteAsyncConnection db, IEnumerable<IClientPushSynchro> clientPushSynchro, IEnumerable<TypeToSync> typeToSyncs)
        {
            this.httpClient = httpClient;
            this.db = db;
            this.clientPushSynchro = clientPushSynchro;
            this.jsonTypeInfoResolver = new EntityTypeResolver(typeToSyncs.Select(c=>c.Type).ToArray());
        }

        public Guid DeviceId { get; set; }

        public async Task Run(int version = 0, Dictionary<string, string> headers = null)
        {
            await this.SyncEvents(SyncEvent.Begin, null);

            try
            {
                var beginLocalDateTime = DateTime.UtcNow;
                var synchroDevice = await this.Begin(headers);

                await this.Push(headers, synchroDevice, version);

                var pullEntities = await this.Pull(headers, version);

                synchroDevice.LastLocalSyncDateTime = beginLocalDateTime;
                synchroDevice.Version = version;
                await this.End(headers, synchroDevice);

                await this.SyncEvents(SyncEvent.End, pullEntities);
            }
            catch (HttpRequestException requestException)
            {
                await this.SyncEvents(SyncEvent.End, new List<IEntity>());
            }
        }

        private async Task SyncEvents(SyncEvent @event, List<IEntity> entities)
        {
            await (this.syncCallback?.SyncEvents(@event, entities) ?? Task.CompletedTask);
        }

        private async Task End(Dictionary<string, string> headers, SynchroDevice synchroDevice)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"/synchro/end?deviceId={this.DeviceId}"))
            {
                if (headers != null)
                {
                    foreach (var key in headers.Keys)
                    {
                        httpRequestMessage.Headers.Add(key, headers[key]);
                    }
                }

                var json = await Serializer.Serialize(synchroDevice, this.jsonTypeInfoResolver);

                httpRequestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
                
                await this.httpClient.SendAsync(httpRequestMessage);
            }
        }

        private async Task<SynchroDevice> Begin(Dictionary<string, string> headers)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                $"/synchro/begin?deviceId={this.DeviceId}"))
            {
                if (headers != null)
                {
                    foreach (var key in headers.Keys)
                    {
                        httpRequestMessage.Headers.Add(key, headers[key]);
                    }
                }
                
                var httpResponseMessage = await this.httpClient.SendAsync(httpRequestMessage);
                

                var synchroDevice = await Serializer.Deserialize<SynchroDevice>(await httpResponseMessage.Content.ReadAsStreamAsync(), this.jsonTypeInfoResolver);

                return synchroDevice;               
            }
        }

        private async Task Push(Dictionary<string, string> headers, SynchroDevice synchroDevice, int version)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"/synchro?deviceId={this.DeviceId}&version={version}"))
            {
                if (headers != null)
                {
                    foreach (var key in headers.Keys)
                    {
                        httpRequestMessage.Headers.Add(key, headers[key]);
                    }
                }
                var lastUpdated = synchroDevice.LastLocalSyncDateTime;
                var entities = await this.GetPushEntities(lastUpdated);

                var json = await Serializer.Serialize(entities, this.jsonTypeInfoResolver);
                
                httpRequestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
                
                await this.httpClient.SendAsync(httpRequestMessage);
            }
        }

        private async Task<List<IEntity>> GetPushEntities(DateTime lastUpdated)
        {
            var entities = new List<IEntity>();
            
            foreach (var pushSynchro in this.clientPushSynchro)
            {
                entities.AddRange(await pushSynchro.GetEntities(lastUpdated));
            }

            return entities;
        }

        private async Task<List<IEntity>> Pull(Dictionary<string, string> headers, int version)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                $"/synchro?deviceId={this.DeviceId}&version={version}"))
            {
                if (headers != null)
                {
                    foreach (var key in headers.Keys)
                    {
                        httpRequestMessage.Headers.Add(key, headers[key]);
                    }
                }
                
                var httpResponseMessage = await this.httpClient.SendAsync(httpRequestMessage);
                
                var cars = await Serializer.Deserialize<List<IEntity>>(await httpResponseMessage.Content.ReadAsStreamAsync(), this.jsonTypeInfoResolver);

                foreach (var car in cars)
                {
                    if (car.IsDeleted)
                    {
                        await this.db.DeleteAsync(car);
                    }
                    else
                    {
                        await this.db.InsertOrReplaceAsync(car);
                    }
                }

                return cars;
            }
        }

        public async Task SetCallback(ISyncCallback syncCallback)
        {
            this.syncCallback = syncCallback;
        }
    }
}