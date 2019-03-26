using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Raven.Client.Documents.Session;
using Talk.EsBase.EventSourcing;
using Talk.EsBase.Server.Infrastructure.EventStore;
using Talk.EsBase.Server.Infrastructure.RavenDb;
using Talk.EsBase.Server.Services;
using static Talk.EsBase.Server.Infrastructure.EventStore.EventStoreConfiguration;
using static Talk.EsBase.Server.Infrastructure.Logging.Logger;
using static Talk.EsBase.Server.Infrastructure.RavenDb.RavenDbConfiguration;

namespace Talk.EsBase.Server
{
    public class Startup
    {

        public Startup(
            ILoggerFactory loggerFactory,
            IWebHostEnvironment environment,
            IConfiguration configuration
        )
        {
            LoggerFactory = loggerFactory;
            Environment = environment;
            Configuration = configuration;
        }

        ILoggerFactory LoggerFactory { get; }
        IWebHostEnvironment Environment { get; }
        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            UseLoggerFactory(LoggerFactory);

            var esConnection = ConfigureEsConnection(
                Configuration["EventStore:ConnectionString"],
                Environment.ApplicationName);
            var documentStore = ConfigureRavenDb(
                Configuration["RavenDb:Server"],
                Configuration["RavenDb:Database"]
            );
            var ravenDbProjectionManager = new ProjectionManager(
                esConnection,
                new RavenDbCheckpointStore(GetSession, "readmodels"),
                ConfigureRavenDbProjections(GetSession)
            );

            services.AddSingleton(c => (Func<IAsyncDocumentSession>) GetSession);
            services.AddSingleton<IHostedService>(
                new EventStoreService(
                    esConnection,
                    ravenDbProjectionManager
                )
            );

            services.AddGrpc();

            IAsyncDocumentSession GetSession() => documentStore.OpenAsyncSession();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMetricServer();
            app.UseHttpMetrics();

            app.UseRouting(routes => { routes.MapGrpcService<EventProcessorService>(); });
        }

        static IProjection[] ConfigureRavenDbProjections(
            Func<IAsyncDocumentSession> getSession)
            => new IProjection[]
                { };
    }
}