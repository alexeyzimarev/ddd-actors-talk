using System;
using System.Threading.Tasks;
using Talk.Messages.Sensor;

namespace Talk.EsBase.Commands.Modules.Vehicles
{
    public static class TelemetryReactor
    {
        public static Task React(
            object @event,
            Func<object, Task> sendCommand)
            => @event switch
                {
                    Events.SensorTelemetryReceived e =>
                        sendCommand(
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