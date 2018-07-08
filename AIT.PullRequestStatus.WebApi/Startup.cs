using AIT.PullRequestStatus.DataAccess.Repositories;
using AIT.PullRequestStatus.Domain.Services;
using AIT.PullRequestStatus.WebApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;

namespace AIT.PullRequestStatus.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(c =>
            {
                c.AllowAnyHeader();
                c.AllowAnyMethod();
                c.AllowAnyOrigin();
                c.AllowCredentials();
            });

            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (!string.IsNullOrEmpty(Configuration["TableStorageKey"]))
            {
                services.AddSingleton(
                    new CloudStorageAccount(
                        new StorageCredentials(
                        Configuration["TableStorageAccount"],
                        Configuration["TableStorageKey"]), true));

                services.AddSingleton<IConfigurationRepository, ConfigurationRepository>();
            }
            else
            {
                services.AddSingleton<IConfigurationRepository, ConfigurationRepositoryLocal>();
            }

            services.AddSingleton<IVssConnectionFactory, VssConnectionFactory>();
            services.AddSingleton<IStatusPoliciesService, StatusPoliciesService>();
            services.AddSingleton<IPullRequestService>(c =>
                new PullRequestService(
                    new Uri($"{Configuration["BaseUrl"]}/api/v1/"),
                    c.GetService<IVssConnectionFactory>(),
                    c.GetService<IStatusPoliciesService>(),
                    c.GetService<IConfigurationRepository>()));

            services.AddCors();

            services.AddMvc()
                .AddApplicationPart(typeof(ServiceHookController).Assembly);
        }
    }
}