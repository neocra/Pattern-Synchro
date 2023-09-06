using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;
using Pattern.Synchro.Api;
using Pattern.Synchro.Sample.Api;
using Xunit;
using Car = Pattern.Synchro.Sample.Client.Car;

namespace Pattern.Synchro.Tests
{
    public class PullCarTests : BaseTests
    {
        public PullCarTests(WebApplicationFactory<Startup> factory, DbContextClassFixture dbContextClassFixture) : base(factory, dbContextClassFixture)
        {
        }

        [Fact]
        public async Task Should_Have_Car_In_Local_Database_When_Pull_From_Server()
        {
            await this.AddServer(new Sample.Api.Car
            {
                Id = Guid.NewGuid(),
                Name = "Megane IV",
                UserId = "1"
            });

            await this.client.Run();

            await this.AssertLocal<Car>(c => "Megane IV" == c.Name);
        }
        
        [Fact]
        public async Task Should_Have_Car_In_Local_Database_When_Pull_From_Server_And_Filter_From_UserId()
        {
            await this.AddServer(new Sample.Api.Car
            {
                Id = Guid.NewGuid(),
                Name = "Megane IV",
                UserId = "1"
            });
            
            await this.AddServer(new Sample.Api.Car
            {
                Id = Guid.NewGuid(),
                Name = "Peugeot",
                UserId = "2"
            });

            await this.client.Run();

            await this.AssertLocal<Car>(c => "Megane IV" == c.Name);
            await this.AssertHaveOne<Car>();
        }
        
        [Fact]
        public async Task Should_Car_In_Local_Is_Updated_When_Pull_From_Server()
        {
            await this.AddServer(new Device
            {
                Id = this.deviceId,
                LastSynchro = new DateTime(2020, 4, 17),
                LastLocalSynchro = new DateTime(2020, 4, 17)
            });
            
            var newGuid = Guid.NewGuid();
            await this.AddServer(new Sample.Api.Car
            {
                Id = newGuid,
                Name = "Megane IV",
                UserId = "1",
                LastUpdated = new DateTime(2020, 4, 18)
            });

            await this.AddLocal(new Car
            {
                Id = newGuid,
                Name = "Megane 4"
            });

            await this.client.Run();

            await this.AssertLocal<Car>(c => "Megane IV" == c.Name);
        }       
        
        [Fact]
        public async Task Should_Car_In_Server_Is_Updated_When_Push_To_Server()
        {
            this.datimeService.DateTimeNow().Returns(new DateTime(2019, 2, 2, 10, 01, 00));
            await this.AddServer(new Device
            {
                Id = this.deviceId,
                LastSynchro =  new DateTime(2019, 2, 2, 10, 00, 45)
            });
            
            var newGuid = Guid.NewGuid();
            await this.AddServer(new Sample.Api.Car
            {
                Id = newGuid,
                Name = "Megane IV",
                UserId = "1",
                LastUpdated = new DateTime(2019, 2, 2, 10, 00, 00)
            });

            await this.AddLocal(new Car
            {
                Id = newGuid,
                Name = "Megane IV",
            });

            await this.client.Run();
            
            await this.AddLocal(new Car
            {
                Id = newGuid,
                Name = "Megane 4",
                LastUpdated = DateTime.Now
            });

            await this.client.Run();

            await this.AssertLocal<Car>(c => "Megane 4" == c.Name);
            await this.AssertServer<Sample.Api.Car>(c => "Megane 4" == c.Name);
        }       
    }
}