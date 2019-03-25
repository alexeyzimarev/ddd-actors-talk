using System.Threading.Tasks;
using Grpc.Core;
using Processor;

namespace Talk.EsBase.Server.Services
{
    public class EventProcessorService : EventReceiver.EventReceiverBase
    {
        public override Task<Ack> ReceiveEvent(Telemetry request, ServerCallContext context)
        {
            return Task.FromResult(new Ack
            {
                Message = "Hello " + request.DeviceId
            });
        }
    }
}