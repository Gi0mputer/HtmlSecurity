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

                // Imposta il token in un cookie HttpOnly
                Response.Cookies.Append("auth_token", tokenString, new CookieOptions
                {
                    HttpOnly = true,           // Non accessibile da JavaScript
                    Secure = false,             // con true Invia il cookie solo su HTTPS
                    SameSite = SameSiteMode.None, // o Strict
                    Expires = DateTime.UtcNow.AddHours(1)
                });


                //In ambienti di produzione la prassi più sicura è avere il client e il server(o l'autenticatore)
                //configurati per operare sullo stesso dominio o, in alternativa, impostare correttamente il dominio del cookie.
                //Nel caso in cui l'autenticatore giri su un dominio diverso da quello in cui sono ospitate le pagine
                //(ad esempio, API su "api.example.com" e client su "www.example.com"),
                //occorre configurare i cookie(ad esempio, impostando il parametro Domain)
                //e scegliere opportunamente il valore di SameSite per permettere la trasmissione dei cookie nelle richieste cross - domain.

                // PER LO SVILUPPO
                //spesso si rilassano temporaneamente le restrizioni(disabilitando Secure e usando SameSite = Lax)
                //per facilitare il testing,

                // IN PRODUZIONE
                //è preferibile usare HTTPS e impostazioni più restrittive per aumentare la sicurezza.
                // Viene restituito un messaggio di conferma senza includere il token nel body (l’autenticazione avviene tramite il cookie).
                //

                return Ok(new { message = "Login riuscito!" });
            }

            return Unauthorized();
        }

        // Endpoint per il check dell'autenticazione
        [HttpGet("check")]
        public IActionResult CheckAuth()
        {
            // Legge il cookie "auth_token"
            if (Request.Cookies.TryGetValue("auth_token", out var token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);
                try
                {
                    // Validazione del token
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
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
                    // In caso di errore (token scaduto o alterato)
                    return Unauthorized(new { message = "Token non valido", error = ex.Message });
                }
            }
            else
            {
                return Unauthorized(new { message = "Token mancante" });
            }
        }
    }

    public class UserLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
