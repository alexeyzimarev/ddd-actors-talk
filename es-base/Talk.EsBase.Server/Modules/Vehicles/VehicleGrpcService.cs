using System.Threading.Tasks;
using Grpc.Core;
using Talk.EventSourcing;
using VehiclesManagement;

namespace Talk.EsBase.Server.Modules.Vehicles
{
    public class VehicleGrpcService : VehicleService.VehicleServiceBase
    {
        readonly VehicleCommandService _appService;

        public VehicleGrpcService(VehicleCommandService appService) => _appService = appService;

        public override async Task<Ack> Register(RegisterVehicle request, ServerCallContext context)
        {
            await _appService.Handle(request);
            return new Ack();
        }

        public override async Task<Ack> AdjustMaxSpeed(AdjustMaximumSpeed request, ServerCallContext context)
        {
            await _appService.Handle(request);
            return new Ack();
        }

        public override async Task<Ack> AdjustMaxTemp(AdjustMaxTemperature request, ServerCallContext context)
        {
            await _appService.Handle(request);
            return new Ack();
        }
    }
}