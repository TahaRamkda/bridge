
using Microsoft.Extensions.Options;
using Serilog;
using System.Configuration;
using WhatsAppBridge.Handler;
using WhatsAppBridge.Middleware;
using WhatsAppBridge.Models;
using WhatsAppBridge.Settings;

namespace WhatsAppBridge
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Add support to logging with SERILOG
            builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext());

            // Add services to the container. 

            //DO NOT CHANGE ORDER OF THE SERVICES

            //Settings
            builder.Services.Configure<WhatsAppConfigurationSetting>(builder.Configuration.GetSection(WhatsAppConfigurationSetting.ConfigKey));
            builder.Services.Configure<IntegrationConfigurationSettings>(builder.Configuration.GetSection(IntegrationConfigurationSettings.ConfigKey));

            //Global HttpClient
            builder.Services.AddHttpClient(HttpClientType.facebook_graph_api, (serviceProvider, httpClient) =>
            {
                var whatsAppConfiguration = serviceProvider.GetRequiredService<IOptions<WhatsAppConfigurationSetting>>().Value;

                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {whatsAppConfiguration.AccessToken}");
                httpClient.BaseAddress = new Uri(whatsAppConfiguration.BaseURL);
                httpClient.Timeout = TimeSpan.FromSeconds(whatsAppConfiguration.TimeOutInSeconds);
            });

            builder.Services.AddHttpClient(HttpClientType.integration_api, (serviceProvider, httpClient) =>
            {
                var whatsAppConfiguration = serviceProvider.GetRequiredService<IOptions<IntegrationConfigurationSettings>>().Value;

                //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {whatsAppConfiguration.AccessToken}");
                httpClient.BaseAddress = new Uri(whatsAppConfiguration.BaseURL);
                httpClient.Timeout = TimeSpan.FromSeconds(whatsAppConfiguration.TimeOutInSeconds);
            });

            builder.Services.AddScoped<WhatsAppWebhookHandler>();
            builder.Services.AddScoped<IntegrationHandler>();
            builder.Services.AddScoped<WhatsAppHandler>();

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            bool enableGlobalExceptionHandler = Convert.ToBoolean(builder.Configuration["EnableGlobalExceptionHandler"]);

            if (enableGlobalExceptionHandler)
                app.UseExceptionHandlerMiddleware();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseSwagger((c =>
                {
                    c.RouteTemplate = "/consulttechies/swagger/{documentName}/swagger.json";
                }));
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/consulttechies/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = "whatsappbridge"; // Custom route
                });
            }

            app.UseHttpsRedirection();

            app.UseCors();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
