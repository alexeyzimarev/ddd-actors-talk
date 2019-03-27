using System.Threading.Tasks;
using Grpc.Core;
using SensorsHandling;
using Ack = SensorsHandling.Ack;

namespace Talk.EsBase.Server.Modules.Sensors
{
    public class SensorGrpcService : SensorService.SensorServiceBase
    {
        public override Task<Ack> Install(SensorInstallation request, ServerCallContext context)
        {
            return base.Install(request, context);
        }

        public override Task<Ack> ReceiveTelemetry(SensorTelemetry request, ServerCallContext context)
        {
            return base.ReceiveTelemetry(request, context);
        }
    }
}