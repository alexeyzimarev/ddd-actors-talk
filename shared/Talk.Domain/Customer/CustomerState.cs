using Talk.EventSourcing;
using static System.String;
using static Talk.Messages.Customer.Events;

namespace Talk.Domain.Customer
{
    public class CustomerState : AggregateState<CustomerState>
    {
        string DisplayName { get; set; }

        public override CustomerState When(CustomerState state, object @event)
            => @event switch
                {
                    CustomerRegistered e =>
                        With(state, x =>
                        {
                            x.Id = e.CustomerId;
                            x.DisplayName = e.DisplayName;
                        }),
                    _ => state
                };

        public override string GetStreamName(string id)
            => $"Customer-{id}";

        protected override bool EnsureValidState(CustomerState newState)
            => !IsNullOrWhiteSpace(newState.Id)
               && !IsNullOrWhiteSpace(newState.DisplayName);
    }
}