using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;
using Pattern.Synchro.Api;
using Pattern.Synchro.Sample.Api;
using Xunit;

namespace Pattern.Synchro.Tests
{
    public class PushDeleteCarTests : BaseTests
    {
        public PushDeleteCarTests(WebApplicationFactory<Startup> factory, DbContextClassFixture dbContextClassFixture) : base(factory, dbContextClassFixture)
        {
        }

        [Fact]
        public async Task Should_Delete_Car_On_Mobile_When_Pull_Deleted_Car()
        {
            this.datimeService.DateTimeNow().Returns(new DateTime(2019, 2, 2, 10, 01, 00));
            await this.AddServer(new Device
            {
                Id = this.deviceId,
                LastSynchro =  new DateTime(2019, 2, 2, 10, 00, 45)
            });
            
            var newGuid = Guid.NewGuid();
            await this.AddLocal(new Sample.Client.Car
            {
                Id = newGuid,
                Name = "Megane IV",
                IsDeleted = true
            }).ConfigureAwait(false);
            
            await this.AddServer(new Car
            {
                Id = newGuid,
                Name = "Megane IV",
                UserId = "1",
                IsDeleted = false,
            }).ConfigureAwait(false);

            await this.client.Run().ConfigureAwait(false);

            await this.AssertServer<Car>(c => c.IsDeleted).ConfigureAwait(false);
        }
    }
}