using Amazon;
using Amazon.S3;
using Amazon.SecretsManager;
using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Interfaces.UnitOfWorks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service.Repositories;
using Service.Services;
using Service.UnitOfWorks;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApi.AuthorizationHandlers;
using WebApi.AuthorizationRequirements;

namespace WebApi
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
            services
                .AddAuthorization(_ => _.AddPolicy("UserProfile", policy => policy.Requirements.Add(new UserProfileRequirement())))
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(_ =>
                {
                    _.Authority = Configuration["Auth0:Domain"];
                    _.Audience = Configuration["Auth0:WebAudience"];
                });

            services.AddCors(options =>
            {
                options.AddPolicy("time-tracker-cors", _ =>
                {
                    _.SetIsOriginAllowedToAllowWildcardSubdomains()
                        .WithOrigins("http://localhost:8080", "https://*.yizhang-paul.com")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("Content-Disposition");
                });
            });

            services.AddControllers().AddJsonOptions(_ => _.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
            services.AddDbContext<TimeTrackerDbContext>(_ => _.UseNpgsql(Configuration["TimeTrackerDbConnectionString"]));
            services.AddScoped<TimeTrackerDbContext, TimeTrackerDbContext>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
            services.AddScoped<IInterruptionItemRepository, InterruptionItemRepository>();
            services.AddScoped<ITaskItemRepository, TaskItemRepository>();
            services.AddScoped<IEventHistoryRepository, EventHistoryRepository>();
            services.AddScoped<IEventHistorySummaryRepository, EventHistorySummaryRepository>();
            services.AddScoped<IEventPromptRepository, EventPromptRepository>();
            services.AddScoped<IUserUnitOfWork, UserUnitOfWork>();
            services.AddScoped<IWorkItemUnitOfWork, WorkItemUnitOfWork>();
            services.AddScoped<IEventUnitOfWork, EventUnitOfWork>();
            services.AddScoped<IAmazonSecretsManager>(_ => new AmazonSecretsManagerClient(RegionEndpoint.USEast1));
            services.AddScoped(_ => new AmazonS3Client(RegionEndpoint.USEast1));
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IInterruptionItemService, InterruptionItemService>();
            services.AddScoped<ITaskItemService, TaskItemService>();
            services.AddScoped<IEventSummaryService, EventSummaryService>();
            services.AddScoped<IEventTrackingService, EventTrackingService>();
            services.AddScoped<IAuthorizationHandler, UserProfileHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler(_ => _.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                var payload = new { Error = $"{exception.Message} {exception.StackTrace}" };
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }));

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("time-tracker-cors");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
