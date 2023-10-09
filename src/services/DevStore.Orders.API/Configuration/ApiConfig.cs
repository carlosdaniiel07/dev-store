using DevStore.Orders.Infra.Context;
using DevStore.WebAPI.Core.Identity;
using DevStore.WebAPI.Core.DatabaseFlavor;
using static DevStore.WebAPI.Core.DatabaseFlavor.ProviderConfiguration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DevStore.WebAPI.Core.Configuration;
using DevStore.Core.Http;
using DevStore.Core.Messages.Integration;
using System;

namespace DevStore.Orders.API.Configuration
{
    public static class ApiConfig
    {
        public static void AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureProviderForContext<OrdersContext>(DetectDatabase(configuration));
            services.AddSingleton<IRestClient, RestClient>();
            services.AddHttpClient(nameof(OrderInitiatedIntegrationEvent), options =>
            {
                options.BaseAddress = new Uri("https://localhost:5461/payments/authorize");
            });
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy("Total",
                    builder =>
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
            });

            services.AddDefaultHealthCheck(configuration);
        }

        public static void UseApiConfiguration(this WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Under certain scenarios, e.g minikube / linux environment / behind load balancer
            // https redirection could lead dev's to over complicated configuration for testing purpouses
            // In production is a good practice to keep it true
            if (app.Configuration["USE_HTTPS_REDIRECTION"] == "true")
                app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("Total");

            app.UseAuthConfiguration();

            app.UseDefaultHealthcheck();

            app.MapControllers();
        }
    }
}