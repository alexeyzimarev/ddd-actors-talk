using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using MassTransit;
using MassTransit.AspNetCoreIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Proto;
using Talk.Actors.Commands.Infrastructure.EventStore;
using Talk.Actors.Commands.Infrastructure.MassTransit;
using Talk.Actors.Commands.Infrastructure.Prometheus;
using Talk.Actors.Commands.Infrastructure.ProtoActor;
using Talk.Actors.Commands.Modules.Customers;
using Talk.Actors.Commands.Modules.Sensors;
using Talk.Actors.Commands.Modules.Vehicles;
using static Talk.Actors.Commands.Infrastructure.EventStore.EventStoreConfiguration;
using EventHandler = Talk.EventSourcing.EventHandler;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Talk.Actors.Commands
{
    public class Startup
    {
        public Startup(
            IHostingEnvironment environment,
            IConfiguration configuration,
            ILoggerFactory loggerFactory
        )
        {
            Environment = environment;
            Configuration = configuration;
            LoggerFactory = loggerFactory;
        }

        IHostingEnvironment Environment { get; }
        IConfiguration Configuration { get; }
        ILoggerFactory LoggerFactory { get; }

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
                ConfigureReactors()
            );

            services.AddSingleton<IHostedService>(
                new EventStoreService(
                    esConnection,
                    reactorsSubscriptionManager)
            );

            Log.SetLoggerFactory(LoggerFactory);
            services.AddSingleton<IHostedService>(provider =>
                new ProtoClusterHostedService(
                    new Uri(Configuration["Proto:ConsulUrl"]),
                    Configuration["Proto:ClusterName"],
                    "localhost",
                    Configuration.GetValue<int>("Proto:NodePort"),
                    esConnection));
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

        static EventHandler[] ConfigureReactors() =>
            new EventHandler[] { TelemetryReactor.React };

        static void MapEvents()
        {
            Modules.Customers.EventMapping.Map();
            Modules.Vehicles.EventMapping.Map();
            Modules.Sensors.EventMapping.Map();
        }

        static string GetLocalIpAddress()
        {
            var addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            return addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)?.ToString();
        }
    }
}