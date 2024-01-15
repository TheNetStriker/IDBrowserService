using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Code
{
    /// <summary>
    /// Helper to enable multiple asp.net pipelines
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Sets up an application branch with an isolated DI container
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="path">Relative path for the application branch</param>
        /// <param name="servicesConfiguration">DI container configuration</param>
        /// <param name="appBuilderConfiguration">Application pipeline configuration for the created branch</param>
        public static IApplicationBuilder UseBranchWithServices(this IApplicationBuilder app, PathString path,
            Action<IServiceCollection> servicesConfiguration, Action<IApplicationBuilder> appBuilderConfiguration)
        {
            return app.UseBranchWithServices(new[] { path }, servicesConfiguration, appBuilderConfiguration);
        }

        /// <summary>
        /// Sets up an application branch with an isolated DI container with several routes (entry points)
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="paths">Relative paths for the application branch</param>
        /// <param name="servicesConfiguration">DI container configuration</param>
        /// <param name="appBuilderConfiguration">Application pipeline configuration for the created branch</param>
        public static IApplicationBuilder UseBranchWithServices(this IApplicationBuilder app, IEnumerable<PathString> paths,
            Action<IServiceCollection> servicesConfiguration, Action<IApplicationBuilder> appBuilderConfiguration)
        {
            WebApplicationOptions options = new() { Args = [] };
            var webHostBuilder = WebApplication.CreateEmptyBuilder(options);

            webHostBuilder.Services.AddSingleton<IServer, DummyServer>();

            webHostBuilder.Host.UseSerilog(Log.Logger);

            servicesConfiguration.Invoke(webHostBuilder.Services);

            var webHost = webHostBuilder.Build();

            var serviceProvider = webHost.Services;
            var serverFeatures = new FeatureCollection();

            var appBuilderFactory = serviceProvider.GetRequiredService<IApplicationBuilderFactory>();
            var branchBuilder = appBuilderFactory.CreateBuilder(serverFeatures);
            var factory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            branchBuilder.Use(async (context, next) =>
            {
                var oldServiceProvider = context.RequestServices;
                using (var scope = factory.CreateScope())
                {
                    context.RequestServices = scope.ServiceProvider;

                    var httpContextAccessor = context.RequestServices
                                                     .GetService<IHttpContextAccessor>();

                    if (httpContextAccessor != null)
                        httpContextAccessor.HttpContext = context;

                    await next();
                }
                context.RequestServices = oldServiceProvider;
            });

            appBuilderConfiguration(branchBuilder);

            var branchDelegate = branchBuilder.Build();

            foreach (var path in paths)
            {
                app.Map(path, builder =>
                {
                    builder.Run(async (context) =>
                    {
                        await branchDelegate(context);
                    });
                });
            }

            // Enables hosted services.
            webHost.Start();

            return app;
        }

        private class DummyServer : IServer
        {
            public IFeatureCollection Features { get; } = new FeatureCollection();

            public void Dispose() { }

            public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken) => Task.CompletedTask;

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }
    }
}
