using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Pattern.Synchro.Api;
using Pattern.Synchro.Client;
using Pattern.Synchro.Sample.Api;
using SQLite;
using Xunit;
using Car = Pattern.Synchro.Sample.Client.Car;

namespace Pattern.Synchro.Tests
{
    [Collection("Tests")]
    public class BaseTests : IClassFixture<WebApplicationFactory<Startup>>, IClassFixture<DbContextClassFixture>, IDisposable
    {
        private readonly DbContextClassFixture dbContextClassFixture;
        private readonly HttpClient httpClient;
        private readonly string serverDatabaseName;
        protected SQLiteAsyncConnection localDb;
        protected SynchroClient client;
        private string localDatabaseName;
        protected Guid deviceId;
        protected IDateTimeService datimeService;
        protected IServerCallback serverCallback;

        public BaseTests(WebApplicationFactory<Startup> factory, DbContextClassFixture dbContextClassFixture)
        {
            this.dbContextClassFixture = dbContextClassFixture;
            this.deviceId = Guid.NewGuid();
            this.serverDatabaseName = this.GetType().Name;
            this.localDatabaseName = this.GetType().Name + "local";
            this.httpClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    this.datimeService = Substitute.For<IDateTimeService>();
                    services.AddTransient(c => this.datimeService);
                    this.serverCallback = Substitute.For<IServerCallback>();
                    services.AddTransient(c => this.serverCallback);
                    services.AddDbContext<SampleDbContext>(opt =>
                        opt.UseSqlite($"Data Source={this.serverDatabaseName}"));
                });
            }).CreateClient();
            
            this.httpClient.DefaultRequestHeaders.Add("UserId", "1");

            dbContextClassFixture.InitDbContext(this.serverDatabaseName);
            this.localDb = new SQLiteAsyncConnection(this.localDatabaseName);
            Task.WaitAll(this.localDb.CreateTableAsync<Car>());

            this.client = new SynchroClient(this.httpClient, this.localDb,
                new[] {new ClientPushSynchro<Car>(this.localDb)});

            this.client.DeviceId = deviceId;
        }

       

        protected async Task AddServer<T>(T obj) where T : class
        {
            var db = this.dbContextClassFixture.DbContext;
            await db.Set<T>().AddAsync(obj);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
        }

        protected async Task AddLocal<T>(T obj)
        {
            await this.localDb.InsertOrReplaceAsync(obj);
        }
        
        protected async Task AssertLocal<T>(Func<T, bool> predicate) where T : new()
        {
            var entity = await this.localDb.Table<T>().ToListAsync();

            Xunit.Assert.NotNull(entity);
            Xunit.Assert.True(entity.Count(predicate) == 1);
        }
        
        protected Task AssertHaveOne<T>() where T : new()
        {
            return this.AssertHave<T>(1);
        }

        protected async Task AssertHave<T>(int count) where T : new()
        {
            var entity = await this.localDb.Table<T>().ToListAsync();

            Xunit.Assert.NotNull(entity);
            Xunit.Assert.True(entity.Count == count);
        }

        protected async Task AssertServer<T>(Func<T, bool> predicate) where T : class, new()
        {
            var db = this.dbContextClassFixture.DbContext;

            var entity = await db.Set<T>().ToListAsync();

            Xunit.Assert.NotNull(entity);
            Xunit.Assert.True(entity.Count(predicate) == 1);
        }

        public void Dispose()
        {
            Task.WaitAll(this.localDb.CloseAsync());
            File.Delete(this.serverDatabaseName);
            File.Delete(this.localDatabaseName);
        }
    }
}