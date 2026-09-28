using RECETASMD.Data;
using RECETASMD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RECETASMD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null || user.Password != dto.Password)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Credenciales incorrectas. Verifique su correo o contraseña."
                });
            }

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Inicio de sesión exitoso",
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email
                }
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (existing)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Ya existe un usuario registrado con este correo electrónico."
                });
            }

            var newUser = new User
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Password = dto.Password,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Registro completado con éxito",
                User = new UserDto
                {
                    Id = newUser.Id,
                    Name = newUser.Name,
                    Email = newUser.Email
                }
            });
        }
    }
}
