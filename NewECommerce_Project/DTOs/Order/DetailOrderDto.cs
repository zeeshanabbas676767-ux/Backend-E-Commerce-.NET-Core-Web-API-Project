using NewECommerce_Project.Models;

namespace NewECommerce_Project.DTOs.Order
{
    public class DetailOrderDto
    {
        public int Id { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DetailOrderItemDto> Items { get; set; }
    }
}
 