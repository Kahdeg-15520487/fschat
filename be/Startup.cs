using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using be.DAL;
using Microsoft.EntityFrameworkCore;
using be.Services;

public class Startup
{
    public Startup(IWebHostEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

        if (env.IsDevelopment())
        {
            builder.AddUserSecrets<Startup>();
        }

        builder.AddEnvironmentVariables();
        Configuration = builder.Build();
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ChatDataContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetIsOriginAllowed((host) => { Console.WriteLine(host); return true; })
                       .AllowCredentials(); ;
            });
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = Configuration["Auth0:Authority"];
            options.Audience = Configuration["Auth0:Audience"];

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        (path.StartsWithSegments("/chatHub")))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
        services.AddScoped<IChatService, ChatService>();
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors("AllowAll");
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ChatHub>("/chatHub");
        });
    }
}
