using Microsoft.AspNetCore.Mvc;
using NewECommerce_Project.Data;
using Microsoft.EntityFrameworkCore;

namespace NewECommerce_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _db;
        public CategoryController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        public async Task<IActionResult> GET() 
        {
            var category = await _db.Categories.ToListAsync();

            return Ok(category);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Models.Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = category.CategoryId }, category);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Models.Category category)
        {
            if (id != category.CategoryId) 
            {
                return BadRequest();
            }
            _db.Entry(category).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}
