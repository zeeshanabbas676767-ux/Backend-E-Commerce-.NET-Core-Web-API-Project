namespace NewECommerce_Project.Models
{
    public enum PaymentStatus 
    {
        Initiated,   // Payment intent created
        Succeeded,   // Money received
        Failed,      // Payment failed
        Refunded     // Money returned (later)
    }
}
