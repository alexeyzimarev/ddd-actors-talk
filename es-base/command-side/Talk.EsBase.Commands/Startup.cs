using MassTransit;
using MassTransit.AspNetCoreIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Talk.EsBase.Commands.Infrastructure.EventStore;
using Talk.EsBase.Commands.Infrastructure.MassTransit;
using Talk.EsBase.Commands.Infrastructure.Prometheus;
using Talk.EsBase.Commands.Modules.Customers;
using Talk.EsBase.Commands.Modules.Sensors;
using Talk.EsBase.Commands.Modules.Vehicles;
using Talk.EventSourcing;
using static Talk.EsBase.Commands.Infrastructure.EventStore.EventStoreConfiguration;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Talk.EsBase.Commands
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
            MapEvents();

            var esConnection = ConfigureEsConnection(
                Configuration["EventStore:ConnectionString"],
                Environment.ApplicationName);

            var store =
                new MeasuredStore(
                    new AggregateStore(esConnection));

            var customerService = new CustomerCommandService(store);
            var vehicleService = new VehicleCommandService(store);
            var sensorService = new SensorCommandService(store);

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

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMetricServer();
            app.UseHttpMetrics();
        }

        static EventHandler[] ConfigureReactors(
            IPublishEndpoint publishEndpoint
        )
        {
            return new EventHandler[]
            {
                @event => TelemetryReactor.React(
                    @event, cmd => publishEndpoint.Publish(cmd))
            };
        }

        static void MapEvents()
        {
            Modules.Customers.EventMapping.Map();
            Modules.Vehicles.EventMapping.Map();
            Modules.Sensors.EventMapping.Map();
        }
    }
}