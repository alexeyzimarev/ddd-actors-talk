using Talk.Domain.Sensor;
using Talk.EsBase.Server.Infrastructure.EventStore;
using Talk.Messages.Sensor;

namespace Talk.EsBase.Server.Modules.Sensors
{
    public static class EventMapping
    {
        public static void Map()
        {
            TypeMapper.Map<Events.SensorInstalled>("SensorInstalled");
            TypeMapper.Map<Events.TelemetryReceived>("TelemetryReceived");
        }
    }
}