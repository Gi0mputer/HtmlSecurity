using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Text;
using static System.Net.WebRequestMethods;

namespace AutenticationServer.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _secretKey = "ChiaveSegretaMoltoLungaCheSupera32Byte"; // Assicurati di usare la stessa chiave della Program.cs

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLogin user)
        {
            if (user.Username == "utente" && user.Password == "password")
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.Name, user.Username)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // NON impostiamo più un cookie, ma restituiamo il token nel body della risposta.
                return Ok(new { token = tokenString });
            }

            return Unauthorized();
        }

        [HttpGet("check")]
        public IActionResult CheckAuth()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Unauthorized(new { message = "Token mancante" });
            }

            string authHeader = Request.Headers["Authorization"];
            if (!authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Formato del token non valido" });
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                return Ok(new { message = "Token valido" });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = "Token non valido", error = ex.Message });
            }
        }

    }

    public class UserLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
