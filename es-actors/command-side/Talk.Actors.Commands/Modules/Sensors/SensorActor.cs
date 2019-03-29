using Proto.Persistence;
using Talk.Actors.Commands.Infrastructure.ProtoActor;
using Talk.Domain.Sensor;
using Talk.Proto.Messages.Commands;

namespace Talk.Actors.Commands.Modules.Sensors
{
    public class SensorActor : ServiceActor<SensorState>
    {
        protected SensorActor(IEventStore store) : base(store)
        {
            When<SensorInstallation>(
                cmd => Sensor.Install(
                    cmd.SensorId,
                    cmd.VehicleId
                )
            );

            When<SensorTelemetry>(
                cmd => Sensor.ReceiveTelemetry(
                    State,
                    cmd.Speed,
                    cmd.Temperature
                )
            );
        }
    }
}