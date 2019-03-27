namespace Talk.Domain.Customer
{
    public static class Events
    {
        public class CustomerRegistered
        {
            public string CustomerId { get; set; }
            public string DisplayName { get; set; }
        }
    }
}