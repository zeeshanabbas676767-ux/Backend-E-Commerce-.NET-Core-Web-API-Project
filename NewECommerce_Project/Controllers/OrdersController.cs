using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NewECommerce_Project.Data;
using NewECommerce_Project.Models;
using NewECommerce_Project.DTOs;
using Microsoft.EntityFrameworkCore;
using NewECommerce_Project.DTOs.Order;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace NewECommerce_Project.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders.Include(o => o.Items).ToListAsync();
            var result = orders.Select(o => new DetailOrderDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UserFullName = o.UserFullName,
                UserEmail = o.UserEmail,
                Items = o.Items.Select(i => new DetailOrderItemDto
                {
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.ImageUrl,                 
                }).ToList() 
            });
                
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var result = new DetailOrderDto
            {
                Id = order.Id,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                UserFullName = order.UserFullName,
                UserEmail = order.UserEmail,
                Items = order.Items.Select(i => new DetailOrderItemDto
                {
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };

            return Ok(result);
        }

            [HttpPost]
            public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
            {
                // DTO validation handled automatically by [ApiController]

                var productIds = dto.Items.Select(i => i.ProductId).ToList();

                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                if (products.Count != productIds.Count)
                    return BadRequest("One or more products not found.");

                if (dto.Items.Any(i => i.Quantity <= 0))
                    return BadRequest("Quantity must be greater than zero.");

                foreach (var item in dto.Items)
                {
                    var product = products.First(p => p.Id == item.ProductId);

                    if (product.Stock < item.Quantity)
                        return BadRequest($"Not enough stock for {product.Name}");
                }

            // 🔐 SAFE USER EXTRACTION
            //int userId = User.GetUserId();
            //string email = User.GetEmail();
            //string fullName = User.GetFullName();
            var userId = HttpContext.Session.GetInt32("UserId");
            var email = HttpContext.Session.GetString("UserEmail");
            var fullName = HttpContext.Session.GetString("UserFullName");
            if(userId == null || email == null || fullName == null)
                return Unauthorized("Please login or register");

            // 2. Calculate TotalAmount
            decimal totalAmount = dto.Items.Sum(i =>
                {
                    var product = products.First(p => p.Id == i.ProductId);
                    return product.Price * i.Quantity;
                });

                var order = new Order
                {
                    UserId = userId.Value,
                    UserEmail = email,
                    UserFullName = fullName,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Items = dto.Items.Select(i =>
                    {
                        var product = products.First(p => p.Id == i.ProductId);
                        product.Stock -= i.Quantity;

                        return new OrderItem
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Price = product.Price,
                            Quantity = i.Quantity,
                            ImageUrl = product.ImageUrl
                        };
                    }).ToList()
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new DetailOrderDto
                {
                    Id = order.Id,
                    UserFullName = fullName,
                    UserEmail = email,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    CreatedAt = order.CreatedAt,
                    Items = order.Items.Select(i => new DetailOrderItemDto
                    {
                        ProductName = i.ProductName,
                        Price = i.Price,
                        Quantity = i.Quantity,
                        ImageUrl = i.ImageUrl
                    }).ToList()
                });
            }

        //[HttpPost]
        //public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        //{
        //    var productIds = dto.Items.Select(i => i.ProductId).ToList();

        //    var products = await _context.Products
        //        .Where(p => productIds.Contains(p.Id))
        //        .ToListAsync();

        //    if (dto == null || dto.Items == null || !dto.Items.Any())
        //        return BadRequest("Order must contain at least one item.");

        //    if (dto.Items.Any(i => i.Quantity <= 0))
        //        return BadRequest("Quantity must be greater than zero.");

        //    foreach (var item in dto.Items)
        //    {
        //        var product = products.FirstOrDefault(p => p.Id == item.ProductId);

        //        if (product == null)
        //            return BadRequest($"Product {item.ProductId} not found");

        //        if (product.Stock < item.Quantity)
        //            return BadRequest($"Not enough stock for {product.Name}");
        //    }

        //    // 2. Calculate TotalAmount
        //    decimal totalAmount = 0;

        //    foreach (var item in dto.Items)
        //    {
        //        var product = products.First(p => p.Id == item.ProductId);
        //        totalAmount += product.Price * item.Quantity;
        //    }

        //    // 🔐 SAFE USER EXTRACTION
        //    int userId = User.GetUserId();
        //    string email = User.GetEmail();
        //    string fullName = User.GetFullName();
        //    //var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    //var userName = User.FindFirstValue(ClaimTypes.Name);
        //    //var userEmail = User.FindFirstValue(ClaimTypes.Email);

        //    if (userId == null)
        //        return Unauthorized("Please login or register");

        //    // Ensure userName and userEmail are not null
        //    if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
        //        return BadRequest("User information is incomplete.");

        //    // 3. Create Order entity
        //    var order = new Order
        //    {
        //        UserId = userId,
        //        UserFullName = fullName,
        //        UserEmail = email,
        //        TotalAmount = totalAmount,
        //        Status = OrderStatus.Pending,
        //        CreatedAt = DateTime.UtcNow,
        //        Items = dto.Items.Select(i =>
        //        {
        //            var product = products.First(p => p.Id == i.ProductId);
        //            return new OrderItem
        //            {
        //                ProductId = product.Id,
        //                ProductName = product.Name,
        //                Price = product.Price,
        //                Quantity = i.Quantity,
        //                ImageUrl = product.ImageUrl
        //            };
        //        }).ToList()
        //    };

        //    // 4. Reserve stock (optional)
        //    foreach (var item in order.Items)
        //    {
        //        var product = products.First(p => p.Id == item.ProductId);
        //        product.Stock -= item.Quantity;
        //    }

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    // 5. Map to DetailOrderDto for return
        //    var result = new DetailOrderDto
        //    {
        //        Id = order.Id,
        //        UserFullName = fullName,
        //        UserEmail = email,
        //        TotalAmount = order.TotalAmount,
        //        Status = order.Status,
        //        CreatedAt = order.CreatedAt,
        //        Items = order.Items.Select(i => new DetailOrderItemDto
        //        {
        //            ProductName = i.ProductName,
        //            Price = i.Price,
        //            Quantity = i.Quantity,
        //            ImageUrl = i.ImageUrl
        //        }).ToList()
        //    };

        //    return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, result);
        //}
        [HttpPatch("{orderId}/status")]
        public IActionResult UpdateStatus(int orderId, [FromBody] OrderStatus newStatus)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) return NotFound();

            order.Status = newStatus;
            _context.SaveChanges();
            return Ok(order);
        }


        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            // 1️⃣ Load order WITH items
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            // 2️⃣ Validate order
            if (order == null)
                return NotFound();
            
            // 3️⃣ Validate status
            if (order.Status == OrderStatus.Shipped)
                return BadRequest("Order already shipped");

            if (order.Status == OrderStatus.Cancelled)
                return BadRequest("Order already cancelled");

            // 4️⃣ Restore stock
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                product.Stock += item.Quantity;
               // if (product != null) product.Stock += item.Quantity;
            }
            
            // 5️⃣ Change status
            order.Status = OrderStatus.Cancelled;

            // 6️⃣ Save
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> PayOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.Items)
                   .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.PaymentFailed)
            {
                return BadRequest("Only pending or failed orders can be paid.");
            }
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product.Stock < item.Quantity)
                    return BadRequest($"Not enough stock for {product.Name}.");
            }
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                    product.Stock += item.Quantity;
            }
            order.Status = OrderStatus.Paid;
           // order.PaidAt = DateTime.UtcNow;  // optional timestamp
            await _context.SaveChangesAsync();
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
                    Quantity = i.Quantity,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };

            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Orders.FirstOrDefaultAsync(u => u.Id == id);
            if (product == null) return NotFound();
            _context.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}



