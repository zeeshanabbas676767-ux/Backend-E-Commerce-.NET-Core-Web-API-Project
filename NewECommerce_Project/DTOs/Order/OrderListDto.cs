using NewECommerce_Project.Models;

namespace NewECommerce_Project.DTOs.Order
{
    public class OrderListDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
