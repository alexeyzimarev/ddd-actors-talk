using System.Threading.Tasks;
using Talk.Domain.Customer;
using Talk.EventSourcing;
using static Talk.Messages.Customer.Commands;

namespace Talk.EsBase.Server.Modules.Customers
{
    public class CustomerCommandService : CommandService<CustomerState>
    {
        public CustomerCommandService(IAggregateStore store)
            : base(store) { }

        public Task Handle(RegisterCustomer command)
            => Handle(
                command.CustomerId,
                state => Customer.Register(
                    command.CustomerId,
                    command.DisplayName
                )
            );
    }
}