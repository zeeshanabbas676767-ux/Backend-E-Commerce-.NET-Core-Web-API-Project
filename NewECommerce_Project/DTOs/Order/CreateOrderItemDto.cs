using System.ComponentModel.DataAnnotations;

namespace NewECommerce_Project.DTOs.Order
{
    public class CreateOrderItemDto
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}
