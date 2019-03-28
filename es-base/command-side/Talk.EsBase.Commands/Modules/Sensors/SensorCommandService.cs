using System.Threading.Tasks;
using Talk.Domain.Sensor;
using Talk.EventSourcing;
using static Talk.Messages.Sensor.Commands;

namespace Talk.EsBase.Commands.Modules.Sensors
{
    public class SensorCommandService : CommandService<SensorState>
    {
        public SensorCommandService(IAggregateStore store)
            : base(store) { }

        public Task Handle(SensorInstallation command)
            => Handle(
                command.SensorId,
                state => Sensor.Install(
                    command.SensorId,
                    command.VehicleId
                )
            );

        public Task Handle(SensorTelemetry command)
            => Handle(
                command.SensorId,
                state => Sensor.ReceiveTelemetry(
                    state,
                    command.Speed,
                    command.Temperature
                )
            );
    }
}