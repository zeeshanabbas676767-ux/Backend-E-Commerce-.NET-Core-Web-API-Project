using System.ComponentModel.DataAnnotations;
using NewECommerce_Project.Models;

namespace NewECommerce_Project.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required]
        public List<CreateOrderItemDto> Items { get; set; }
    }

}
