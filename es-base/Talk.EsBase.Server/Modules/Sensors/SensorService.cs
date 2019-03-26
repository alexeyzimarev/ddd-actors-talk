using System.Threading.Tasks;
using Grpc.Core;
using Telemetry;

namespace Talk.EsBase.Server.Modules.Sensors
{
    public class SensorService : TelemetryProcessor.TelemetryProcessorBase
    {
        public override Task<Ack> Receive(SensorTelemetry request, ServerCallContext context)
        {
            return Task.FromResult(new Ack
            {
                Message = "Hello " + request.DeviceId
            });
        }
    }
}