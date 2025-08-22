using Microsoft.AspNetCore.Mvc;
using ProjectForm.Models;
using ProjectForm.Contracts; // DTO’ları içeri al

[ApiController]
[Route("[controller]")] 
public class HomeController : ControllerBase
{
    private readonly InternDBcontext _context;
    public HomeController(InternDBcontext context) => _context = context;

    [HttpPost("login")]
[ProducesResponseType(typeof(object), 200)]
[ProducesResponseType(401)]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Email ve şifre zorunlu." });

        var user = _context.Users.FirstOrDefault(u => u.email == dto.Email && u.password == dto.Password);
        if (user == null)
            return Unauthorized(new { message = "Geçersiz kimlik bilgileri." });

        // Oturum (Session) örneği
        HttpContext.Session.SetInt32("UserId", user.id);

        // ✅ iOS için ad-soyadı direkt döndürüyoruz (Welcome ekranında kullanacaksın)
        return Ok(new
        {
            // İstersen message'ı da bırakıyorum
            message = "Giriş başarılı",
            userId = user.id,
            firstName = user.name ?? string.Empty,
            lastName = user.surname ?? string.Empty
        });
    }

    
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(409)]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Email ve şifre zorunlu." });

        if (_context.Users.Any(u => u.email == dto.Email))
            return Conflict(new { message = "Bu email zaten mevcut" });

        var tokens = (dto.FullName ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var name = tokens.FirstOrDefault();
        var surname = string.Join(" ", tokens.Skip(1));

        var user = new UsersModel
        {
            email = dto.Email,
            password = dto.Password,
            name = name,
            surname = string.IsNullOrEmpty(surname) ? null : surname
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok(new { message = "Kayıt başarılı", userId = user.id });
    }
}