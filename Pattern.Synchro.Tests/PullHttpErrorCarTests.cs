using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Pattern.Synchro.Client;
using Pattern.Synchro.Sample.Api;
using Xunit;
using Car = Pattern.Synchro.Sample.Client.Car;

namespace Pattern.Synchro.Tests
{
    public class PullHttpErrorCarTests : BaseTests
    {
        public PullHttpErrorCarTests(WebApplicationFactory<Startup> factory, DbContextClassFixture dbContextClassFixture) : base(factory, dbContextClassFixture)
        {
        }

        [Fact]
        public async Task Should_Have_Catch_TimeOut_When_Pull_From_Server()
        {
            await this.AddServer(new Sample.Api.Car
            {
                Id = Guid.NewGuid(),
                Name = "Megane IV",
                UserId = "1"
            }).ConfigureAwait(false);

            var httpClient1 = new HttpClient(new ErrorHttpHandler());
            httpClient1.BaseAddress = new Uri("http://localhost/");
            this.client = new SynchroClient(httpClient1, this.localDb,
                new[] {new ClientPushSynchro<Car>(this.localDb)}, new []
                {
                    new TypeToSync(typeof(Car))
                });

            this.client.DeviceId = deviceId;

            await this.client.Run().ConfigureAwait(false);

            await this.AssertHave<Car>(0).ConfigureAwait(false);
        }
    }

    public class ErrorHttpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new System.Net.Http.HttpRequestException("Connection timed out")
            {

            };
        }
    }
}