namespace NewECommerce_Project.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string Provider { get; set; }
        public string GatewayTransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
