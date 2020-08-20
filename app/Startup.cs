using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ST.Data;
using ST.Models;
using ST.WebApi.Repos;
using System.Text;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Notifications;
using WebApi.Services;

namespace ST.WebApi
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCors();
      var connectionString = Configuration.GetConnectionString("WebApiDatabaseExt");
      services.AddDbContext<AppDbContext>(o =>
              //o.UseSqlServer(connectionString)
              o.UseMySql(connectionString)
      );
      services.AddHttpContextAccessor();
      services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

      // configure strongly typed settings objects
      var appSettingsSection = Configuration.GetSection("AppSettings");
      services.Configure<AppSettings>(appSettingsSection);

      // configure jwt authentication
      var appSettings = appSettingsSection.Get<AppSettings>();
      var key = Encoding.ASCII.GetBytes(appSettings.Secret);
      services.AddAuthentication(x =>
      {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(x =>
      {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = false,
          ValidateAudience = false
        };

        x.Events = new JwtBearerEvents
        {
          OnMessageReceived = context =>
          {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/roomsHub")))
            {
              // Read the token out of the query string
              context.Token = accessToken;
            }
            return Task.CompletedTask;
          }
        };
      });

      //services.Configure<CookieAuthenticationOptions>(o =>
      //{
      //  o.LoginPath = PathString.Empty;
      //});

      //services.AddIdentity<ApplicationUser, IdentityRole>(options =>
      //{
      //  options.User.RequireUniqueEmail = true;
      //});

      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
      services.AddSignalR();

      // configure DI for application services
      services.AddScoped<IUserService, UserService>();
      services.AddScoped<IAppData, AppData>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      //if (env.IsDevelopment())
      //{
        
      //}

      app.UseDeveloperExceptionPage();

      app.UseCors(builder => builder
      .AllowAnyHeader()
      .AllowAnyMethod()
      .SetIsOriginAllowed((host) => true)
      .AllowCredentials());

      app.UseAuthentication();

      app.UseSignalR(routes =>
      {
        routes.MapHub<RoomsHub>("/roomsHub");
      });

      app.UseMvc();
    }

  }
}
