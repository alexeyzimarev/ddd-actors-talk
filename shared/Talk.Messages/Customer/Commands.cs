namespace Talk.Messages.Customer
{
    public static class Commands
    {
        public class RegisterCustomer
        {
            public string CustomerId { get; set; }
            public string DisplayName { get; set; }
        }
    }
}