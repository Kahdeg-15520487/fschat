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
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using DinkToPdf.Contracts;
using DinkToPdf;
using Azure.Identity;

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

        if (env.IsProduction())
        {
            var ttt = Configuration["KeyVaultUrl"];
            builder.AddAzureKeyVault(new Uri(Configuration["KeyVaultUrl"]), new DefaultAzureCredential());
        }
        Configuration = builder.Build();
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        Console.WriteLine(Configuration.GetConnectionString("DefaultConnection"));

        services.AddDbContext<ChatDataContext>(options =>
        {
            if (false && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
            }
            else
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                //options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=fschat;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
            }
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetIsOriginAllowed((host) => { return true; })
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
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = ClaimTypes.NameIdentifier
            };

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

        services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ChatDataContext dbContext)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        dbContext.Database.Migrate();

        app.UseCors("AllowAll");
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ChatHub>("/chatHub");
            endpoints.MapControllers();
        });
    }
}
