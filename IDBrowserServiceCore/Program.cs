using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using IDBrowserServiceCore.Installers;
using IDBrowserServiceCore.Services;
using IDBrowserServiceCore.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

if (args.Length > 0 && !args[0].Equals("%LAUNCHER_ARGS%"))
{
    CommandLineHandler.Handle(args);
}
else
{
    IConfiguration Configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("sites.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    IDBrowserConfiguration configuration = new();
    Configuration.Bind(configuration);

    bool openIdEnabled = !string.IsNullOrEmpty(configuration.OpenId.ConfigurationAddress);

    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.ConfigureKestrel(options =>
    {
        var kestrelSection = builder.Configuration.GetSection("Kestrel");
        options.Configure(kestrelSection);
        kestrelSection.Bind(options);
    });
    builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(Configuration));

    var fusionCacheBuilder = builder.Services.AddFusionCache()
        .WithDefaultEntryOptions(options =>
        {
            options.Duration = TimeSpan.FromMinutes(1);
        })
        .WithSerializer(new FusionCacheSystemTextJsonSerializer());

    if (configuration.RedisConnection != null)
    {
        fusionCacheBuilder
            .WithDistributedCache(
                new RedisCache(new RedisCacheOptions() { Configuration = configuration.RedisConnection })
            );
    }

    if (openIdEnabled)
    {
        builder.Services.AddSingleton(provider =>
        {
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                configuration.OpenId.ConfigurationAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());

            return configManager;
        });
    }

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSerilogRequestLogging();

    try
    {
        if (configuration.Sites != null)
        {
            foreach (KeyValuePair<string, SiteSettings> siteKeyValuePair in configuration.Sites)
            {
                string strSiteName = siteKeyValuePair.Key;
                SiteSettings siteSettings = siteKeyValuePair.Value;
                ConnectionStringSettings connectionStringSettings = siteSettings.ConnectionStrings;
                ServiceSettings serviceSettings = siteSettings.ServiceSettings;
                OpenIdSettings openIdSettings = configuration.OpenId;

                siteSettings.SiteName = strSiteName;

                //https://www.strathweb.com/2017/04/running-multiple-independent-asp-net-core-pipelines-side-by-side-in-the-same-application/
                app.UseBranchWithServices("/" + strSiteName,
                    services =>
                    {
                        // Shared services
                        services.AddSingleton(sp => app.Services.GetRequiredService<IFusionCache>());
                        
                        if (openIdEnabled)
                        {
                            services
                                .AddAuthentication(options =>
                                {
                                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                                })
                                .AddJwtBearer(o =>
                                {
                                    var configManager = app.Services
                                        .GetRequiredService<ConfigurationManager<OpenIdConnectConfiguration>>();

                                    o.ConfigurationManager = configManager;

                                    o.TokenValidationParameters = new TokenValidationParameters
                                    {
                                        ValidAudience = openIdSettings.Audience,
                                        ValidateIssuerSigningKey = true,
                                        ValidateIssuer = true,
                                        ValidateAudience = true,
                                        ValidateLifetime = true,
                                    };

                                    if (app.Environment.IsDevelopment())
                                    {
                                        o.RequireHttpsMetadata = false;
                                        o.IncludeErrorDetails = true;
                                    }

                                    if (openIdSettings.DisableServerCertificateValidation)
                                    {
                                        o.BackchannelHttpHandler = new HttpClientHandler
                                        {
                                            ServerCertificateCustomValidationCallback = delegate
                                            {
                                                return true;
                                            }
                                        };
                                    }
                                });

                            services.AddAuthorizationBuilder()
                                .AddDefaultPolicy("Default", policy =>
                                {
                                    policy.RequireRole("site-" + strSiteName);
                                    policy.AddRequirements(new BlacklistSessionRequirement());
                                });

                            services.AddSingleton<IAuthorizationHandler, BlacklistSessionHandler>();
                        }

                        services
                            .AddMvc(option =>
                            {
                                option.EnableEndpointRouting = false;

                                if (!openIdEnabled)
                                {
                                    option.Filters.Add(new AllowAnonymousFilter());
                                }
                            })
                            .AddXmlSerializerFormatters();

                        services.AddScoped<IDatabaseCache, DatabaseCache>();
                        services.AddSingleton(siteSettings);
                        services.AddSingleton(serviceSettings);
                        services.AddSingleton(openIdSettings);

                        if (configuration.UseSwagger)
                        {
                            //Register the Swagger generator, defining 1 or more Swagger documents
                            services.AddSwaggerGen(c =>
                            {
                                c.SwaggerDoc("v1", new OpenApiInfo
                                {
                                    Title = $"IDBrowserService Core Site \"{strSiteName}\"",
                                    Version = "v1",
                                    Description = "Webservice for IDBrowser Android app."
                                });

                                if (openIdEnabled)
                                {
                                    c.AddSecurityDefinition("OpenId", new OpenApiSecurityScheme
                                    {
                                        Type = SecuritySchemeType.OpenIdConnect,
                                        OpenIdConnectUrl = new Uri(openIdSettings.ConfigurationAddress),
                                    });

                                    OpenApiSecurityScheme openIdSecurityScheme = new()
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Id = "OpenId",
                                            Type = ReferenceType.SecurityScheme,
                                        },
                                        In = ParameterLocation.Header,
                                        Name = "Bearer",
                                        Scheme = "Bearer",
                                    };

                                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                                    {
                                        { openIdSecurityScheme, Array.Empty<string>() },
                                    });
                                }

                                // Set the comments path for the Swagger JSON and UI.
                                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                                if (File.Exists(xmlPath))
                                    c.IncludeXmlComments(xmlPath);
                            });
                        }

                        if (configuration.UseResponseCompression)
                            services.AddResponseCompression(options =>
                            {
                                options.Providers.Add<BrotliCompressionProvider>();
                                options.Providers.Add<GzipCompressionProvider>();
                                options.EnableForHttps = true;
                            });

                        services.AddDbContextPool<IDImagerDB>(options => StaticFunctions
                            .SetDbContextOptions(options, connectionStringSettings.DBType, connectionStringSettings.IDImager));
                        services.AddDbContextPool<IDImagerThumbsDB>(options => StaticFunctions
                            .SetDbContextOptions(options, connectionStringSettings.DBType, connectionStringSettings.IDImagerThumbs));

                        services.AddCronSchedulerServices(serviceSettings, strSiteName);
                    },
                    appBuilder =>
                    {
                        if (openIdEnabled)
                        {
                            appBuilder.UseAuthentication();
                            appBuilder.UseAuthorization();
                        }

                        if (configuration.UseResponseCompression)
                            appBuilder.UseResponseCompression();

                        appBuilder.UseMvc();

                        if (configuration.UseSwagger)
                        {
                            // Enable middleware to serve generated Swagger as a JSON endpoint.
                            appBuilder.UseSwagger();

                            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                            // specifying the Swagger JSON endpoint.
                            appBuilder.UseSwaggerUI(c =>
                            {
                                c.SwaggerEndpoint($"/{strSiteName}/swagger/v1/swagger.json", "IDBrowserServiceCore V1");

                                if (openIdEnabled)
                                {
                                    c.OAuthClientId("idbrowser");
                                }
                            });
                        }
                    });
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex.ToString());
    }

    app.MapGet("/", () => "Service online!");

    if (openIdEnabled)
    {
        // OpenID Backchannel logout api
        app.MapPost("logout", async ([FromForm(Name = "logout_token")] string logoutToken,
            [FromServices] IFusionCache cache,
            [FromServices] ConfigurationManager<OpenIdConnectConfiguration> configManager,
            CancellationToken cancellationToken = default) =>
        {
            if (string.IsNullOrEmpty(logoutToken))
            {
                return Results.BadRequest("No logout_token received");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(logoutToken))
            {
                return Results.BadRequest("Invalid logout_token format");
            }

            var config = await configManager.GetConfigurationAsync(CancellationToken.None);

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = config.Issuer,
                ValidAudience = configuration.OpenId.Audience,
                IssuerSigningKeys = config.SigningKeys,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false
            };

            try
            {
                var principal = tokenHandler.ValidateToken(logoutToken, validationParameters, out var validatedToken);
                var sessionId = principal.FindFirst("sid")?.Value;

                var entryOptions = new FusionCacheEntryOptions
                {
                    Duration = TimeSpan.FromHours(1)
                };

                await cache.SetAsync($"session_blacklist:{sessionId}", true, entryOptions, ["session_blacklist"], cancellationToken);

                return Results.Ok();
            }
            catch (SecurityTokenException)
            {
                return Results.Unauthorized();
            }
        }).DisableAntiforgery();
    }

    app.Run();
}