using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Raven.Client.Documents.Session;
using Talk.EsBase.Server.Infrastructure.EventStore;
using Talk.EsBase.Server.Infrastructure.Prometheus;
using Talk.EsBase.Server.Infrastructure.RavenDb;
using Talk.EsBase.Server.Modules.Customers;
using Talk.EsBase.Server.Modules.Projections;
using Talk.EsBase.Server.Modules.Sensors;
using Talk.EsBase.Server.Modules.Vehicles;
using static Talk.EsBase.Server.Infrastructure.EventStore.EventStoreConfiguration;
using static Talk.EsBase.Server.Infrastructure.RavenDb.RavenDbConfiguration;
using EventHandler = Talk.EventSourcing.EventHandler;

namespace Talk.EsBase.Server
{
    public class Startup
    {
        public Startup(
            IWebHostEnvironment environment,
            IConfiguration configuration
        )
        {
            Environment = environment;
            Configuration = configuration;
        }

        IWebHostEnvironment Environment { get; }
        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            PrometheusMetrics.TryConfigure(Environment.ApplicationName);
            MapEvents();

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
            var store =
                new MeasuredStore(
                new AggregateStore(esConnection));

            services.AddSingleton(new CustomerCommandService(store));
            services.AddSingleton(new VehicleCommandService(store));
            services.AddSingleton(new SensorCommandService(store));

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

            app.UseRouting(routes =>
            {
                routes.MapGrpcService<SensorGrpcService>();
                routes.MapGrpcService<VehicleGrpcService>();
                routes.MapGrpcService<CustomerGrpcService>();
            });
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

        static void MapEvents()
        {
            Modules.Customers.EventMapping.Map();
            Modules.Vehicles.EventMapping.Map();
            Modules.Sensors.EventMapping.Map();
        }
    }
}