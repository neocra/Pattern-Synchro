using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;
using Pattern.Synchro.Api;
using Pattern.Synchro.Client;
using Pattern.Synchro.Sample.Api;
using Xunit;
using Car = Pattern.Synchro.Sample.Client.Car;

namespace Pattern.Synchro.Tests
{
    public class DeviceTests : BaseTests
    {
        public DeviceTests(WebApplicationFactory<Startup> factory, DbContextClassFixture dbContextClassFixture) : base(factory, dbContextClassFixture)
        {
        }
        
        [Fact]
        public async Task Should_Version_Is_Updated_On_Server_When_Synchro_End()
        {
            this.datimeService.DateTimeNow().Returns(new DateTime(2019, 2, 2, 10, 00, 45));

            await this.client.Run(1);

            await this.AssertServer<Device>(c => c.Version == 1);
        }

        [Fact]
        public async Task Should_Device_Is_Updated_On_Server_When_Synchro_End()
        {
            this.datimeService.DateTimeNow().Returns(new DateTime(2019, 2, 2, 10, 00, 45));

            await this.client.Run();

            await this.AssertServer<Device>(c => c.LastSynchro == new DateTime(2019, 2, 2, 10, 00, 45));
        }
        
        [Fact]
        public async Task Should_Callback_When_Synchro_Is_End()
        {
            this.datimeService.DateTimeNow().Returns(new DateTime(2019, 2, 2, 10, 00, 45));

            var syncCallback = Substitute.For<ISyncCallback>();
            await this.client.SetCallback(syncCallback);

            await this.client.Run();

            await syncCallback.Received(1).SyncEvents(SyncEvent.End, Arg.Any<List<IEntity>>());
        }

        [Fact]
        public async Task Should_Callback_When_Synchro_Is_Begin()
        {
            this.datimeService.DateTimeNow().Returns(new DateTime(2019, 2, 2, 10, 00, 45));

            var syncCallback = Substitute.For<ISyncCallback>();
            await this.client.SetCallback(syncCallback);

            await this.client.Run();

            await syncCallback.Received(1).SyncEvents(SyncEvent.Begin, Arg.Any<List<IEntity>>());
        }
        
        [Fact]
        public async Task Should_Callback_Contains_Pull_Object_When_Synchro_Is_End()
        {
            await this.AddServer(new Sample.Api.Car
            {
                Id = Guid.NewGuid(),
                Name = "Megane IV",
                UserId = "1"
            });

            var syncCallback = Substitute.For<ISyncCallback>();
            await this.client.SetCallback(syncCallback);

            await this.client.Run();

            await syncCallback.Received(1).SyncEvents(SyncEvent.End, Arg.Is<List<IEntity>>(e => e.Any(c => c is Car)));
        }
        
                
        [Fact]
        public async Task Should_Callback_Contains_Empty_Pull_Object_When_Synchro_Is_End_And_Entity_Are_Already_Pulled()
        {
            var deviceId = Guid.NewGuid();
            await this.AddServer(new Device
            {
                Id = deviceId,
                LastSynchro =  new DateTime(2019, 2, 2, 10, 00, 45)
            });
            await this.AddServer(new Sample.Api.Car
            {
                Id = Guid.NewGuid(),
                Name = "Megane IV",
                UserId = "1",
                LastUpdated = new DateTime(2019, 2, 2, 10, 00, 00)
            });

            this.client.DeviceId = deviceId;
            
            var syncCallback = Substitute.For<ISyncCallback>();
            await this.client.SetCallback(syncCallback);

            await this.client.Run();

            await syncCallback.Received(1).SyncEvents(SyncEvent.End, Arg.Is<List<IEntity>>(e => e.Count == 0));
        }
        
        [Fact]
        public async Task Should_Callback_Server_When_Synchro_Is_Begin()
        {
            var deviceId = Guid.NewGuid();
            await this.AddServer(new Device
            {
                Id = deviceId,
                LastSynchro =  new DateTime(2019, 2, 2, 10, 00, 45)
            });
            await this.AddServer(new Sample.Api.Car
            {
                Id = Guid.NewGuid(),
                Name = "Megane IV",
                UserId = "1",
                LastUpdated = new DateTime(2019, 2, 2, 10, 00, 00)
            });

            this.client.DeviceId = deviceId;
            
            await this.client.Run();

            await this.serverCallback.Received(1).Begin(Arg.Any<IHeaderDictionary>());
        }
        
        [Fact]
        public async Task Should_Callback_Server_Contains_Header_When_Synchro_With_Dictionary()
        {
            var deviceId = Guid.NewGuid();
            await this.AddServer(new Device
            {
                Id = deviceId,
                LastSynchro =  new DateTime(2019, 2, 2, 10, 00, 45)
            });
            await this.AddServer(new Sample.Api.Car
            {
                Id = Guid.NewGuid(),
                Name = "Megane IV",
                UserId = "1",
                LastUpdated = new DateTime(2019, 2, 2, 10, 00, 00)
            });

            this.client.DeviceId = deviceId;

            await this.client.Run(headers: new Dictionary<string, string>()
            {
                {"Key1", "Value1"}
            });

            await this.serverCallback.Received(1).Begin(Arg.Is<IHeaderDictionary>(
                h => h.ContainsKey("Key1") && h["Key1"] == "Value1"));
        }
    }
}