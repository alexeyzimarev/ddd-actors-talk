using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Raven.Client.Documents.Session;
using Talk.EventSourcing;
using Talk.EsBase.Server.Infrastructure.EventStore;
using Talk.EsBase.Server.Infrastructure.Prometheus;
using Talk.EsBase.Server.Infrastructure.RavenDb;
using Talk.EsBase.Server.Modules.Projections;
using Talk.EsBase.Server.Modules.Sensors;
using static Talk.EsBase.Server.Infrastructure.EventStore.EventStoreConfiguration;
using static Talk.EsBase.Server.Infrastructure.Logging.Logger;
using static Talk.EsBase.Server.Infrastructure.RavenDb.RavenDbConfiguration;
using EventHandler = Talk.EventSourcing.EventHandler;

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
            PrometheusMetrics.TryConfigure(Environment.ApplicationName);

            var esConnection = ConfigureEsConnection(
                Configuration["EventStore:ConnectionString"],
                Environment.ApplicationName);
            var documentStore = ConfigureRavenDb(
                Configuration["RavenDb:Server"],
                Configuration["RavenDb:Database"]
            );
            var ravenDbProjectionManager = new SubscriptionManager(
                esConnection,
                new RavenDbCheckpointStore(GetSession, "readmodels"),
                "ravenDbSubscription",
                ConfigureRavenDbProjections(GetSession)
            );

            services.AddSingleton(c => (Func<IAsyncDocumentSession>) GetSession);
            services.AddSingleton<IAggregateStore>(new AggregateStore(esConnection));
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

            app.UseRouting(routes => { routes.MapGrpcService<SensorService>(); });
        }

        static EventHandler[] ConfigureRavenDbProjections(
            Func<IAsyncDocumentSession> getSession)
            => new EventHandler[]
            {
                new RavenDbProjection<ReadModels.VehicleItem>(
                    getSession, VehicleItemProjection.GetHandler).Project,
            };
    }
}