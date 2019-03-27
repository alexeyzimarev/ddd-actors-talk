using System.Threading.Tasks;
using CustomersManagement;
using Grpc.Core;

namespace Talk.EsBase.Server.Modules.Customers
{
    public class CustomerGrpcService : CustomerService.CustomerServiceBase
    {
        readonly CustomerCommandService _appService;

        public CustomerGrpcService(CustomerCommandService appService)
            => _appService = appService;

        public override async Task<Ack> Register(RegisterCustomer request, ServerCallContext context)
        {
            await _appService.Handle(request);
            return new Ack();
        }
    }
}