using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Client.Documents.Session;
using Talk.EsBase.Queries.Infrastructure;
using Talk.EsBase.Queries.Infrastructure.RavenDb;
using Talk.EsBase.Queries.Modules.Projections;
using Talk.EventStore;
using static Talk.EsBase.Queries.Infrastructure.RavenDb.RavenDbConfiguration;
using static Talk.EventStore.EventStoreConfiguration;
using EventHandler = Talk.EventSourcing.EventHandler;

namespace Talk.EsBase.Queries
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            EventMapping.Map();

            var esConnection = ConfigureEsConnection(
                Configuration["EventStore:ConnectionString"],
                "es-talk-queries");
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        }

        static EventHandler[] ConfigureRavenDbProjections(
            Func<IAsyncDocumentSession> getSession
        )
            => new EventHandler[]
            {
                new RavenDbProjection<ReadModels.VehicleItem>(
                    getSession, VehicleItemProjection.GetHandler).Project,
                new RavenDbProjection<ReadModels.CustomerVehicles>(
                    getSession, CustomerVehiclesProjection.GetHandler).Project,
            };
    }
}