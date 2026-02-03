using Microsoft.AspNetCore.Mvc;
using NewECommerce_Project.Data;
using NewECommerce_Project.Models;
using Microsoft.EntityFrameworkCore;
using NewECommerce_Project.DTOs.Product;

namespace NewECommerce_Project.Controllers
{
    [ApiController]                     
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProductsController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Category_Name,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    Stock = p.Stock,
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _db.Products
              .Where(x => x.Id == id && x.IsActive)
              .FirstOrDefaultAsync();

            if (p == null) return NotFound();
            var dto = new ProductDetailDto
            {
                Id= p.Id,  
                Name=p.Name,
                Description=p.Description,
                Price=p.Price,
                ImageUrl=p.ImageUrl,
                CategoryId=p.CategoryId,
                Stock= p.Stock, 
            };
            return Ok(dto);
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
        {
            string imageUrl = "";

            if (dto.ImageUrl != null && dto.ImageUrl.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/images"
                );

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUrl.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.ImageUrl.CopyToAsync(stream);

                imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/images/{fileName}";
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                Stock = dto.Stock,
                ImageUrl = imageUrl,
                IsActive = true
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            var result = new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                Stock = product.Stock
            };
            return CreatedAtAction(nameof(Get), new { id = product.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateDto dto)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.CategoryId = dto.CategoryId;
            product.Stock = dto.Stock;

            if (dto.ImageUrl != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/products");

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUrl.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageUrl.CopyToAsync(stream);
                }

                product.ImageUrl = $"{Request.Scheme}://{Request.Host}/uploads/products/{fileName}";
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products
                            .Where(p => p.Id == id && p.IsActive)
                            .FirstOrDefaultAsync();
            if (product == null) return NotFound();
            product.IsActive = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
