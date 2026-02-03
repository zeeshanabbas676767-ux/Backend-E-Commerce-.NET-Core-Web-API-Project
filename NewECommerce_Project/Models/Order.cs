namespace NewECommerce_Project.Models
{
    public class Order
    { 
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending; // default Pending
        public DateTime CreatedAt { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

}
