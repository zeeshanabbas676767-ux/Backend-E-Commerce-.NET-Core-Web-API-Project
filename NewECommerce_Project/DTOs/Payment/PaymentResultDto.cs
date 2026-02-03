using NewECommerce_Project.Models;

namespace NewECommerce_Project.DTOs.Payment
{
    public class PaymentResultDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }    
        public PaymentStatus Status { get; set; }

    }
}
