namespace Talk.Domain.Customer
{
    public static class Customer
    {
        public static CustomerState.Result Register(
            string customerId,
            string displayName
        )
            => new CustomerState()
                .Apply(new Events.CustomerRegistered
                {
                    CustomerId = customerId,
                    DisplayName = displayName
                });
    }
}