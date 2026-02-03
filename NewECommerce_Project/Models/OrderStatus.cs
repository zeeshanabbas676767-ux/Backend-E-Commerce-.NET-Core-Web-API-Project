namespace NewECommerce_Project.Models
{
    public enum OrderStatus
    {
        Pending = 0,   // Order created but not paid yet
        Paid = 1,      // Payment completed
        PaymentFailed = 2,  // Payment attempt failed
        Shipped = 3,   // Order shipped to customer
        Cancelled = 4// Order cancelled by user/system
    }
}
