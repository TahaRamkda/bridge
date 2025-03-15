
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;
using WhatsAppBridge.Handler;
using WhatsAppBridge.Helpers;
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
            //builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext());
            
            // Get log level from configuration
            var logLevel = builder.Configuration.GetValue<string>("Logging:LogLevel:Default");
            var minLevel = logLevel switch
            {
                "Debug" => LogEventLevel.Debug,
                "Information" => LogEventLevel.Information,
                "Warning" => LogEventLevel.Warning,
                "Error" => LogEventLevel.Error,
                _ => LogEventLevel.Information // Default level
            };

            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(minLevel) // Dynamically apply level
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)  // Suppress low-level framework logs
                .MinimumLevel.Override("System", LogEventLevel.Error)  // Only show Errors for System logs
                .WriteTo.Http(
                    requestUri: builder.Configuration["Axiom:LogURL"],
                    queueLimitBytes: null,
                    httpClient: new CustomHttpClient(),
                    configuration: builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog(logger);

            // Add services to the container. 

            //DO NOT CHANGE ORDER OF THE SERVICES

            //Settings
            builder.Services.Configure<AuthenticationConfigurationSettings>(builder.Configuration.GetSection(AuthenticationConfigurationSettings.ConfigKey));
            builder.Services.Configure<WhatsAppConfigurationSetting>(builder.Configuration.GetSection(WhatsAppConfigurationSetting.ConfigKey));
            builder.Services.Configure<IntegrationConfigurationSettings>(builder.Configuration.GetSection(IntegrationConfigurationSettings.ConfigKey));

            //Global HttpClient
            builder.Services.AddHttpClient(HttpClientType.facebook_graph_api, (serviceProvider, httpClient) =>
            {
                var whatsAppConfiguration = serviceProvider.GetRequiredService<IOptions<WhatsAppConfigurationSetting>>().Value;

                //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {whatsAppConfiguration.AccessToken}");
                httpClient.BaseAddress = new Uri(whatsAppConfiguration.BaseURL);
                httpClient.Timeout = TimeSpan.FromSeconds(whatsAppConfiguration.TimeOutInSeconds);
            });

            builder.Services.AddHttpClient(HttpClientType.integration_api, (serviceProvider, httpClient) =>
            {
                var integrationConfiguration = serviceProvider.GetRequiredService<IOptions<IntegrationConfigurationSettings>>().Value;

                httpClient.DefaultRequestHeaders.Add("X-API-KEY", $"Bearer {integrationConfiguration.ApiKey}");
                httpClient.BaseAddress = new Uri(integrationConfiguration.BaseURL);
                httpClient.Timeout = TimeSpan.FromSeconds(integrationConfiguration.TimeOutInSeconds);
            });

            builder.Services.AddScoped<WhatsAppWebhookHandler>();
            builder.Services.AddScoped<IntegrationHandler>();
            builder.Services.AddScoped<WhatsAppHandler>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "JWT_OR_API_KEY"; // This will allow you to configure multiple schemes
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Use JWT by default for challenge.
            }).AddJwtBearer(options =>
            {
                // JWT Bearer settings go here.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourJWTSecretKeyHere")),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthenticationSchemes.ApiKeyPolicy, policy => policy.RequireAuthenticatedUser().AddAuthenticationSchemes("ApiKey"));
                options.AddPolicy(AuthenticationSchemes.BearerPolicy, policy => policy.RequireAuthenticatedUser().AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
            });

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            //Authentication
            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(c =>
            {
                var schemaHelper = new SwashbuckleSchemaHelper();
                c.CustomSchemaIds(type => schemaHelper.GetSchemaId(type));

                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServiceName", Version = "1" }); 
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Name = "x-api-key",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Authorization by x-api-key inside request's header",
                    Scheme = "ApiKeyScheme"
                });

                var key = new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    },
                    In = ParameterLocation.Header
                };

                var requirement = new OpenApiSecurityRequirement { { key, new List<string>() } };

                c.AddSecurityRequirement(requirement);
            });

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

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
