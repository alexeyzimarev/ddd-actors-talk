using System.Threading.Tasks;
using Talk.Domain.Sensor;
using Talk.EventSourcing;
using Talk.Messages.Sensor;
using static Talk.Messages.Sensor.Commands;

namespace Talk.EsBase.Commands.Modules.Sensors
{
    public class SensorCommandService : CommandService<SensorState>
    {
        public SensorCommandService(IAggregateStore store)
            : base(store)
        {
            When<SensorInstallation>(
                cmd => cmd.SensorId,
                (state, cmd) => Sensor.Install(
                    cmd.SensorId,
                    cmd.VehicleId
                )
            );

            When<SensorTelemetry>(
                cmd => cmd.SensorId,
                (state, cmd) => Sensor.ReceiveTelemetry(
                    state,
                    cmd.Speed,
                    cmd.Temperature
                )
            );
        }
    }
}