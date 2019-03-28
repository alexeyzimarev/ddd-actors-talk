using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Raven.Client.Documents.Session;
using Talk.EsBase.Queries.Infrastructure.EventStore;
using Talk.EsBase.Queries.Infrastructure.Prometheus;
using Talk.EsBase.Queries.Infrastructure.RavenDb;
using Talk.EsBase.Queries.Modules.Projections;
using static Talk.EsBase.Queries.Infrastructure.EventStore.EventStoreConfiguration;
using static Talk.EsBase.Queries.Infrastructure.RavenDb.RavenDbConfiguration;
using EventHandler = Talk.EventSourcing.EventHandler;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Talk.EsBase.Queries
{
    public class Startup
    {
        public Startup(
            IHostingEnvironment environment,
            IConfiguration configuration
        )
        {
            Environment = environment;
            Configuration = configuration;
        }

        IHostingEnvironment Environment { get; }
        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            PrometheusMetrics.TryConfigure(Environment.ApplicationName);
            EventMapping.Map();

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
            services.AddSingleton<IHostedService>(
                new EventStoreService(
                    esConnection,
                    ravenDbProjectionManager
                )
            );

            IAsyncDocumentSession GetSession() => documentStore.OpenAsyncSession();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMetricServer();
            app.UseHttpMetrics();
        }

        static EventHandler[] ConfigureRavenDbProjections(
            Func<IAsyncDocumentSession> getSession)
            => new EventHandler[]
            {
                new RavenDbProjection<ReadModels.VehicleItem>(
                    getSession, VehicleItemProjection.GetHandler).Project,
                new RavenDbProjection<ReadModels.CustomerVehicles>(
                    getSession, CustomerVehiclesProjection.GetHandler).Project,
            };
    }
}