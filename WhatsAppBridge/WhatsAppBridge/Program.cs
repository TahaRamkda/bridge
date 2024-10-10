
using Serilog;
using System.Configuration;
using WhatsAppBridge.Middleware;
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
            builder.Services.Configure<WhatsAppConfigurationSetting>(builder.Configuration.GetSection(WhatsAppConfigurationSetting.ConfigKey));

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

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
