using NewECommerce_Project.Models;

namespace NewECommerce_Project.DTOs.Order
{
    public class DetailOrderItemDto
    {
        public string ImageUrl { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
