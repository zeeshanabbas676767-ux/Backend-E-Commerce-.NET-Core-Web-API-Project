using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewECommerce_Project.Data;
using NewECommerce_Project.Models;
using NewECommerce_Project.DTOs.Payment;
using NewECommerce_Project.DTOs.Order;

namespace NewECommerce_Project.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartPayment(PaymentCreateDto dto)
        {
            // 1️⃣ Load order
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                return NotFound("Order not found");

            // 2️⃣ Validate order status
            if (order.Status != OrderStatus.Pending &&
                order.Status != OrderStatus.PaymentFailed)
            {
                return BadRequest("Order cannot be paid");
            }

            // 3️⃣ Create payment (NEW row every time)
            var payment = new Payment
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Amount = order.TotalAmount,   // FROM DB
                Status = PaymentStatus.Initiated,
                Provider = "MockGateway",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payment.Add(payment);
            await _context.SaveChangesAsync();

            // 4️⃣ Return result
            return Ok(new PaymentResultDto
            {
                PaymentId = payment.Id,
                OrderId = order.Id,
                Status = payment.Status
            });
        }
        [HttpPost("{paymentId}/success")]
        public async Task<IActionResult> PaymentSucceeded(int paymentId)
        {
            // 1️⃣ Load the payment and order
            var payment = await _context.Payment
                .Include(p => p.Order)
                .ThenInclude(o => o.Items)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return NotFound("Payment not found");

            var order = payment.Order;
            if (order == null)
                return BadRequest("Associated order not found");

            // 2️⃣ Validate payment and order status
            if (payment.Status == PaymentStatus.Succeeded)
                return BadRequest("Payment already succeeded");

            if (order.Status == OrderStatus.Shipped)
                return BadRequest("Cannot pay shipped order");

            if (order.Status == OrderStatus.Cancelled)
                return BadRequest("Cannot pay cancelled order");

            if (payment.Amount != order.TotalAmount)
                return BadRequest("Payment amount mismatch");

            // 3️⃣ Check stock again (optional safety)
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product.Stock < item.Quantity)
                    return BadRequest($"Not enough stock for {product.Name}");
            }

            // 4️⃣ Mark payment as succeeded
            payment.Status = PaymentStatus.Succeeded;

            // 5️⃣ Mark order as Paid
            order.Status = OrderStatus.Paid;

            // 6️⃣ Reduce stock permanently if not already done
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                product.Stock -= item.Quantity;  // finalize stock
            }

            await _context.SaveChangesAsync();

            // 7️⃣ Return updated order info
            var result = new DetailOrderDto
            {
                Id = order.Id,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new DetailOrderItemDto
                {
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return Ok(result);
        }


        [HttpPost("{paymentId}/fail")]
        public async Task<IActionResult> PaymentFailed(int paymentId)
        {
            // 1️⃣ Load payment with order and items
            var payment = await _context.Payment
                .Include(p => p.Order)
                .ThenInclude(o => o.Items)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return NotFound("Payment not found");

            var order = payment.Order;
            if (order == null)
                return BadRequest("Associated order not found");

            // 2️⃣ Validate status
            if (payment.Status == PaymentStatus.Succeeded)
                return BadRequest("Cannot fail a succeeded payment");

            if (order.Status == OrderStatus.Shipped)
                return BadRequest("Cannot fail shipped order");

            if (order.Status == OrderStatus.Cancelled)
                return BadRequest("Cannot fail cancelled order");

            // 3️⃣ Mark payment as failed
            payment.Status = PaymentStatus.Failed;

            // 4️⃣ Restore stock if it was reserved during order creation
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                product.Stock += item.Quantity; // put stock back
            }

            // 5️⃣ Keep order status as Pending or mark as PaymentFailed
            order.Status = OrderStatus.PaymentFailed; // optional if you want to track failed attempts

            // 6️⃣ Save all changes
            await _context.SaveChangesAsync();

            // 7️⃣ Return order info with failed payment
            var result = new DetailOrderDto
            {
                Id = order.Id,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new DetailOrderItemDto
                {
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return Ok(result);
        }


        [HttpPost("{orderId}/retry")]
        public async Task<IActionResult> RetryPayment(int orderId)
        {
            // 1️⃣ Load the order with items
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound("Order not found");

            if (order.Status != OrderStatus.PaymentFailed)
                return BadRequest("Only failed orders can be retried.");

            // 2️⃣ Check stock again
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product.Stock < item.Quantity)
                    return BadRequest($"Not enough stock for {product.Name}");
            }

            // 3️⃣ Create new Payment row
            var newPayment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,  // server calculates total
                Status = PaymentStatus.Initiated,
                Provider = "GatewayName",
                CreatedAt = DateTime.UtcNow
            };
            _context.Payment.Add(newPayment);

            // 4️⃣ Reserve stock if needed (depends on business logic)
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                product.Stock -= item.Quantity;
            }

            await _context.SaveChangesAsync();

            // 5️⃣ Return the new payment info
            var result = new PaymentResultDto
            {
                PaymentId = newPayment.Id,
                OrderId = order.Id,
                Status = newPayment.Status
            };

            return Ok(result);
        }
        [HttpPost("{paymentId}/refund")]
        public async Task<IActionResult> RefundPayment(int paymentId)
        {
            // 1️⃣ Load payment and order
            var payment = await _context.Payment
                .Include(p => p.Order)
                .ThenInclude(o => o.Items)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return NotFound("Payment not found");

            var order = payment.Order;
            if (order == null)
                return BadRequest("Associated order not found");

            // 2️⃣ Validate status
            if (payment.Status != PaymentStatus.Succeeded)
                return BadRequest("Only succeeded payments can be refunded");

            if (payment.Status == PaymentStatus.Refunded)
                return BadRequest("Payment already refunded");

            // 3️⃣ Process refund (logic handled by payment gateway / server)
            // Example: call payment provider API here
            bool refundSuccess = true; // replace with actual API result
            if (!refundSuccess)
                return BadRequest("Refund failed at gateway");

            // 4️⃣ Update payment status
            payment.Status = PaymentStatus.Refunded;

            // 5️⃣ Optional: update order status
            order.Status = OrderStatus.Cancelled; // if refund cancels the order

            // 6️⃣ Optional: restore stock if refund includes returned items
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                product.Stock += item.Quantity;
            }

            // 7️⃣ Save changes
            await _context.SaveChangesAsync();

            // 8️⃣ Return updated info
            var result = new DetailOrderDto
            {
                Id = order.Id,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new DetailOrderItemDto
                {
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return Ok(result);
        }


    }
}
