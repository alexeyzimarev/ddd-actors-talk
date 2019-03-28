using System.Threading.Tasks;
using Talk.Domain.Customer;
using Talk.EventSourcing;
using static Talk.Messages.Customer.Commands;

namespace Talk.EsBase.Commands.Modules.Customers
{
    public class CustomerCommandService : CommandService<CustomerState>
    {
        public CustomerCommandService(IAggregateStore store)
            : base(store)
        {
            When<RegisterCustomer>(
                cmd => cmd.CustomerId,
                (state, cmd) => Customer.Register(
                    cmd.CustomerId,
                    cmd.DisplayName
                )
            );
        }
    }
}