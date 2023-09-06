using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Pattern.Synchro.Api;
using Pattern.Synchro.Sample.Api;
using Xunit;
using Car = Pattern.Synchro.Sample.Client.Car;

namespace Pattern.Synchro.Tests
{
    public class PullCarTestsVersion : BaseTests
    {
        public PullCarTestsVersion(WebApplicationFactory<Startup> factory, DbContextClassFixture dbContextClassFixture) : base(factory, dbContextClassFixture)
        {
        }
        
        [Fact]
        public async Task Should_Have_Car_Updated_In_Local_Database_When_Pull_From_Server_And_Version_Different()
        {
            await this.AddServer(new Device
            {
                Id = this.deviceId,
                LastSynchro = new DateTime(2020, 4, 2),
                LastLocalSynchro = new DateTime(2020, 4, 2),
                Version = 1
            });

            var newGuid = Guid.NewGuid();
            await this.AddLocal(new Car
            {
                Id = newGuid,
                Name = "Megane 4",
                LastUpdated = new DateTime(2020, 4, 1)
            });
            
            await this.AddServer(new Sample.Api.Car
            {
                Id = newGuid,
                Name = "Megane IV",
                UserId = "1",
                LastUpdated = new DateTime(2020, 4, 1)
            });

            await this.client.Run(version: 2);

            await this.AssertLocal<Car>(c => "Megane IV" == c.Name);
        }
        
        [Fact]
        public async Task Should_Have_Car_version2_Updated_In_Server_Database_When_Push_From_Local_And_Version_Different()
        {
            var newGuid = Guid.NewGuid();
            await this.AddLocal(new Car
            {
                Id = newGuid,
                Name = "Megane 4",
                LastUpdated = new DateTime(2020, 4, 1)
            });
            
            await this.client.Run(version: 2);

            await this.AssertServer<CarV2>(c => "Megane 4" == c.Name);
        }
    }
}