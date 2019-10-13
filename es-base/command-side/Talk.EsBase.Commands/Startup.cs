using MassTransit;
using MassTransit.AspNetCoreIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Talk.EsBase.Commands.Infrastructure;
using Talk.EsBase.Commands.Modules.Customers;
using Talk.EsBase.Commands.Modules.Sensors;
using Talk.EsBase.Commands.Modules.Vehicles;
using Talk.EventSourcing;
using Talk.EventStore;
using static Talk.EventStore.EventStoreConfiguration;

namespace Talk.EsBase.Commands
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            MapEvents();

            var esConnection = ConfigureEsConnection(
                Configuration["EventStore:ConnectionString"],
                "es-base-commands"
            );

            var store = new AggregateStore(esConnection);

            var customerService = new CustomerCommandService(store);
            var vehicleService  = new VehicleCommandService(store);
            var sensorService   = new SensorCommandService(store);

            var bus =
                MassTransitConfiguration.ConfigureBus(
                    "rabbitmq://localhost", "guest", "guest",
                    ("talk-customer", ep =>
                    {
                        ep.Handler<Messages.Customer.Commands.RegisterCustomer>(
                            ctx => customerService.Handle(ctx.Message));
                    }),
                    ("talk-vehicle", ep =>
                    {
                        ep.Handler<Messages.Vehicle.Commands.RegisterVehicle>(
                            ctx => vehicleService.Handle(ctx.Message));
                        ep.Handler<Messages.Vehicle.Commands.AdjustMaxSpeed>(
                            ctx => vehicleService.Handle(ctx.Message));
                        ep.Handler<Messages.Vehicle.Commands.AdjustMaxTemperature>(
                            ctx => vehicleService.Handle(ctx.Message));
                        ep.Handler<Messages.Vehicle.Commands.RegisterVehicleTelemetry>(
                            ctx => vehicleService.Handle(ctx.Message));
                    }),
                    ("talk-sensor", ep =>
                    {
                        ep.Handler<Messages.Sensor.Commands.SensorInstallation>(
                            ctx => sensorService.Handle(ctx.Message));
                        ep.Handler<Messages.Sensor.Commands.SensorTelemetry>(
                            ctx => sensorService.Handle(ctx.Message));
                    }));

            services.AddMassTransit(bus);

            var reactorsSubscriptionManager = new SubscriptionManager(
                esConnection,
                new EsCheckpointStore(esConnection, "reactors-checkpoint"),
                "commandsReactors",
                ConfigureReactors(bus)
            );

            services.AddSingleton<IHostedService>(
                new EventStoreService(
                    esConnection,
                    reactorsSubscriptionManager)
            );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        }

        static EventHandler[] ConfigureReactors(IPublishEndpoint publishEndpoint)
            => new EventHandler[]
            {
                @event => TelemetryReactor.React(
                    @event, cmd => publishEndpoint.Publish(cmd))
            };

        static void MapEvents()
        {
            Modules.Customers.EventMapping.Map();
            Modules.Vehicles.EventMapping.Map();
            Modules.Sensors.EventMapping.Map();
        }
    }
}