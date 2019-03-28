using System.Threading.Tasks;
using Talk.Actors.Commands.Infrastructure.ProtoActor;
using Talk.Messages.Sensor;

namespace Talk.Actors.Commands.Modules.Vehicles
{
    public static class TelemetryReactor
    {
        public static Task React(object @event)
            => @event switch
                {
                    Events.SensorTelemetryReceived e =>
                        ProtoCluster.SendToActor(
                            "Vehicle",
                            e.VehicleId,
                            new Messages.Vehicle.Commands.RegisterVehicleTelemetry
                            {
                                VehicleId = e.VehicleId,
                                SensorId = e.SensorId,
                                Speed = e.Speed,
                                Temperature = e.Temperature
                            }),
                    _ => Task.CompletedTask
                };
    }
}