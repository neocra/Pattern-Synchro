using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pattern.Synchro.Api;
using Pattern.Synchro.Api.Pull;
using Pattern.Synchro.Api.Push;

namespace Pattern.Synchro.Sample.Api
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSynchro();
            services.AddTransient<IServerPullProvider, CarPullProvider>();
            services.AddTransient<IServerPushProvider, CarPushProvider>();
            services.AddTransient<IServerPushProvider, CarV2PushProvider>();

            services.AddTransient<IDeviceInformation, DeviceInformation>();

            services.AddAuthentication("UserId")
                .AddScheme<UserAuthentificationOptions, UserAuthentification>("UserId", opt => { });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            
            app.UseMiddleware<SynchroMiddleWare>();
            app.Run(async (context) => { await context.Response.WriteAsync("Hello World!"); });
        }
    }

    public class UserAuthentificationOptions : AuthenticationSchemeOptions
    {
    }

    public class UserAuthentification  : AuthenticationHandler<UserAuthentificationOptions>
    {
        public UserAuthentification(IOptionsMonitor<UserAuthentificationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }
              
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var user = this.Request.Headers["UserId"].First();
            
            var claims = new[] { new Claim(ClaimTypes.Name, user) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}