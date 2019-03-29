using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Proto;
using Proto.Persistence.RavenDB;
using Raven.Client.Documents.Session;
using Talk.Actors.Queries.Infrastructure.EventStore;
using Talk.Actors.Queries.Infrastructure.Prometheus;
using Talk.Actors.Queries.Infrastructure.RavenDb;
using Talk.Actors.Queries.Modules.Projections;
using static Talk.Actors.Queries.Infrastructure.EventStore.EventStoreConfiguration;
using static Talk.Actors.Queries.Infrastructure.RavenDb.RavenDbConfiguration;
using EventHandler = Talk.EventSourcing.EventHandler;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Talk.Actors.Queries
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

            var ravenDbStore = new RavenDBProvider(documentStore);

            services.AddSingleton(c => (Func<IAsyncDocumentSession>) GetSession);
            services.AddSingleton<IHostedService>(
                new EventStoreService(
                    esConnection,
                    Props.FromProducer(() =>
                        new SubscriptionActor(
                            esConnection,
                            new RavenDbCheckpointStore(GetSession, "readmodels"),
                            "ravenDbSubscription",
                            (Props.FromProducer(
                                () => new CustomerVehiclesProjection(ravenDbStore)), "customerVehicles"),
                            (Props.FromProducer(
                                () => new VehicleItemProjection(ravenDbStore)), "vehicleItems")
                        )
                    )
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
    }
}