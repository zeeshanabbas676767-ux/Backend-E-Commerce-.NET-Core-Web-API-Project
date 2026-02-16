using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NewECommerce_Project.Data;
using NewECommerce_Project.DTOs;
using NewECommerce_Project.Models;
using Microsoft.AspNetCore.Identity;


namespace NewECommerce_Project.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _config = config;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> AllUsers()
        {
            var user = await _context.Users.ToListAsync();
            var dto = user.Select(p => new AllUserListDto {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                TotalOrders = _context.Orders.Count(o => o.UserId == p.Id)
            });
            return Ok(dto);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already exists");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                CreatedAt = DateTime.Now
            };

            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserFullName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);

            // 🔐 AUTO-LOGIN PART (JWT generation)
            //    var tokenHandler = new JwtSecurityTokenHandler();

            //    var jwtKey = _config["Jwt:Key"];
            //    if (string.IsNullOrWhiteSpace(jwtKey))
            //        throw new InvalidOperationException("JWT key missing");

            //    var key = Encoding.UTF8.GetBytes(jwtKey);

            //    var tokenDescriptor = new SecurityTokenDescriptor
            //    {
            //        Subject = new ClaimsIdentity(new[]
            //        {
            //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            //    new Claim(ClaimTypes.Email, user.Email),
            //    new Claim(ClaimTypes.Name, user.FullName)
            //}),
            //        Expires = DateTime.UtcNow.AddYears(20),
            //        SigningCredentials = new SigningCredentials(
            //            new SymmetricSecurityKey(key),
            //            SecurityAlgorithms.HmacSha256Signature
            //        )
            //    };

            //var token = tokenHandler.CreateToken(tokenDescriptor);
            //var jwtToken = tokenHandler.WriteToken(token);

            return Ok(new
            {
                user = new { user.Id, user.FullName, user.Email },
                //token = jwtToken
            });
        }


        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null) return Unauthorized();

            var hasher = new PasswordHasher<User>();

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserFullName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            //    var tokenHandler = new JwtSecurityTokenHandler();

            //    var jwtKey = _config["Jwt:Key"];
            //    var jwtIssuer = _config["Jwt:Issuer"];
            //    var jwtAudience = _config["Jwt:Audience"];


            //    if (string.IsNullOrWhiteSpace(jwtKey))
            //        throw new InvalidOperationException("JWT key is missing in appsettings.json");
            //    if (string.IsNullOrWhiteSpace(jwtIssuer))
            //        throw new InvalidOperationException("JWT issuer is missing in appsettings.json");
            //    if (string.IsNullOrWhiteSpace(jwtAudience))
            //        throw new InvalidOperationException("JWT audience is missing in appsettings.json");

            //    var key = Encoding.UTF8.GetBytes(jwtKey);


            //    var tokenDescriptor = new SecurityTokenDescriptor
            //    {
            //        Subject = new ClaimsIdentity(new[]
            //        {
            //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            //    new Claim(ClaimTypes.Email, user.Email),
            //    new Claim(ClaimTypes.Name, user.FullName),
            //}),
            //        Expires = DateTime.UtcNow.AddYears(20),
            //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            //    };

            //    var token = tokenHandler.CreateToken(tokenDescriptor);
            //    var jwtToken = tokenHandler.WriteToken(token);

            return Ok(new { user = new { user.Id, user.FullName, user.Email }/*, token = jwtToken*/ });
        }


        //[HttpPost("login")]
        //public IActionResult Login(LoginDto dto)
        //{
        //    var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
        //    if (user == null) return Unauthorized("Invalid Credentials");
        //    if (user.PasswordHash != dto.Password) return Unauthorized("Invalid Password");

        //    HttpContext.Session.SetInt32("UserId", user.Id);
        //    HttpContext.Session.SetString("UserName", user.FullName);
        //    HttpContext.Session.SetString("Email", user.Email);

        //    return Ok(new { user = new { user.Id, user.Email, user.FullName } });
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (product == null) return NotFound();
            _context.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
